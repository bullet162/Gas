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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
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


app.UseHttpsRedirection();

app.UseCors("AllowAll");       // CORS before routing

// app.UseAuthorization();     

app.MapControllers();          // Routes controllers
app.MapHealthChecks("/heathz");


app.Run();