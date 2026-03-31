using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForecastingGas.Data;

/// <summary>
/// Used only by EF Core CLI tools (dotnet ef migrations add).
/// Not used at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Read from env var first, then fall back to the dev connection string
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? "postgresql://neondb_owner:npg_Jna1frlGc9Ou@ep-round-sun-a1pq3far-pooler.ap-southeast-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require";

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
