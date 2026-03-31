using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForecastingGas.Data;

/// <summary>
/// Used only by EF Core CLI tools (dotnet ef migrations / database update).
/// Not used at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var raw = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "postgresql://neondb_owner:npg_Jna1frlGc9Ou@ep-round-sun-a1pq3far-pooler.ap-southeast-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require";

        optionsBuilder.UseNpgsql(ToNpgsqlConnectionString(raw));

        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Converts a postgres:// URI to Npgsql keyword=value connection string.
    /// Npgsql does not accept URI format directly.
    /// </summary>
    private static string ToNpgsqlConnectionString(string uri)
    {
        // Already keyword=value format — return as-is
        if (!uri.StartsWith("postgres://") && !uri.StartsWith("postgresql://"))
            return uri;

        var u = new Uri(uri);
        var userInfo = u.UserInfo.Split(':');
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = u.Host;
        var port = u.Port > 0 ? u.Port : 5432;
        var database = u.AbsolutePath.TrimStart('/');

        // Parse query string for sslmode etc.
        var query = System.Web.HttpUtility.ParseQueryString(u.Query);
        var sslmode = query["sslmode"] ?? "require";

        return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode={sslmode};Trust Server Certificate=true";
    }
}
