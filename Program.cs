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
using ForecastingGas.Utils.numberGenerator;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ISes, ForecastSes>();
builder.Services.AddScoped<IGetData, Data>();
builder.Services.AddScoped<ISaveData, SaveData>();
builder.Services.AddScoped<IHwes, AdditiveHwes>();
builder.Services.AddScoped<IError, Error>();
builder.Services.AddScoped<IDataProvider, Numbers>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");       // CORS before routing

// app.UseAuthorization();        // If you use [Authorize]

app.MapControllers();          // Routes controllers

app.Run();