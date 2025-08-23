using System.Text.Json;
using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Requests;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data;

public class AppDbContext : DbContext
{

    public DbSet<ActualValues> GetActualValues { get; set; }
    public DbSet<DataDescription> GetDataDescriptions { get; set; }

    public DbSet<ForecastDescription> GetForecastDescriptions { get; set; }
    public DbSet<ForecastValues> GetForecastValues { get; set; }
    public DbSet<PredictionValues> GetPredictionValues { get; set; }

    public DbSet<ErrorValues> GetErrorValues { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActualValues>()
       .HasOne(av => av.GetDataDescription)
       .WithMany(dd => dd.ActualValues)
       .HasForeignKey(av => av.DataDescriptionID)
       .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ForecastValues>()
       .HasOne(x => x.GetForecastDescription)
       .WithMany(a => a.GetForecastValues)
       .HasForeignKey(c => c.ForecastDescriptionID)
       .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PredictionValues>()
        .HasOne(p => p.GetForecastDescription)
        .WithMany(fd => fd.GetPredictionValues)
        .HasForeignKey(p => p.ForecastDescriptionID)
        .OnDelete(DeleteBehavior.Cascade);

    }
}