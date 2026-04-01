using ForecastingGas.Data;
using ForecastingGas.Data.Repositories.Implementations;
using ForecastingGas.Data.Repositories.Interfaces;
using ForecastingGas.Algorithm.Interfaces;
using ForecastingGas.Algorithm.Ses;
using Microsoft.EntityFrameworkCore;
using ForecastingGas.Algorithm.Hwes;
using ForecastingGas.Error_Metrics.Interfaces;
using ForecastingGas.Error_Metrics.Service;
using ForecastingGas.Utils.Interfaces;
using ForecastingGas.Utils.Csv;
using ForecastingGas.Algorithm.Gas.Interface;
using ForecastingGas.Algorithm.Gas.Implementations;
using ForecastingGas.Algorithm.Gas.Implementations.Utils;
using ForecastingGas.Repositories.Implementations;
using ForecastingGas.Algorithm;
using ForecastingGas.Utils;
using ForecastingGas.Algorithm.Gas;
using ForecastingGas.Dto.Requests;

static string ToNpgsqlConnectionString(string uri)
{
    if (!uri.StartsWith("postgres://") && !uri.StartsWith("postgresql://"))
        return uri;

    var u = new Uri(uri);
    var userInfo = u.UserInfo.Split(':');
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host = u.Host;
    var port = u.Port > 0 ? u.Port : 5432;
    var database = u.AbsolutePath.TrimStart('/');
    var query = System.Web.HttpUtility.ParseQueryString(u.Query);
    var sslmode = query["sslmode"] ?? "require";

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode={sslmode};Trust Server Certificate=true";
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooonnggggggggggggggg
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache(); //new feature replaces database hehe
builder.Services.AddHealthChecks();
builder.Services.AddScoped<ISes, ForecastSes>();
builder.Services.AddScoped<IGetData, Data>();
builder.Services.AddScoped<ISaveData, SaveData>();
builder.Services.AddScoped<IHwes, AdditiveHwes>();
builder.Services.AddScoped<IError, Error>();
builder.Services.AddScoped<IUploadCsv, UploadCsv>();
builder.Services.AddScoped<IMtGas, MTGas>();
builder.Services.AddScoped<IModel, WeightedForecast>();
builder.Services.AddScoped<IGetForecastValues, GetForecastValues>();
builder.Services.AddScoped<IDeleteForecast, DeleteForecast>();
builder.Services.AddScoped<ISearch, GridSearch>();
builder.Services.AddScoped<IGetError, GetError>();
builder.Services.AddScoped<ITrainTest, TrainTest>();
builder.Services.AddScoped<IDeleteData, DeleteData>();
builder.Services.AddScoped<IWatch, Watch>();
builder.Services.AddScoped<IProcessing, Processing>();
builder.Services.AddSingleton<RawDataCache>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("No database connection string found.");

    options.UseNpgsql(ToNpgsqlConnectionString(connectionString), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://gas-app-algorithm.web.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Local dev – use launchSettings.json or fallback to 5297
    app.Urls.Add("http://localhost:5297");
}
else
{
    // Production (Render) – use Render's provided port
    var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
    app.Urls.Add($"http://*:{port}");
}


app.UseSwagger();
app.UseSwaggerUI();

// Auto-apply pending migrations on startup (handles Render cold deploys)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration failed on startup — check DATABASE_URL is set.");
    }
}


app.UseCors("AllowAll");

// app.UseAuthorization();     

app.MapControllers();          // Routes controllers
app.MapHealthChecks("/heathz");


app.Run();