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
  public DbSet<PredictionValues2> GetPredictionValues2 { get; set; }
  public DbSet<PredictionValues3> GetPredictionValues3 { get; set; }


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
    .HasOne(p => p.GetForecastDescription2)
    .WithMany(fd => fd.GetPredictionValues)
    .HasForeignKey(p => p.ForecastDescriptionID2)
    .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<PredictionValues2>()
        .HasOne(p => p.GetForecastDescription3)
        .WithMany(fd => fd.GetPredictionValues2)
        .HasForeignKey(p => p.ForecastDescriptionID3)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<PredictionValues3>()
       .HasOne(p => p.GetForecastDescription4)
       .WithMany(fd => fd.GetPredictionValues3)
       .HasForeignKey(p => p.ForecastDescriptionID4)
       .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<ErrorValues>()
      .HasOne(e => e.GetForecastDescription5)
      .WithMany(fd => fd.GetErrorValues)
      .HasForeignKey(e => e.ForecastDescriptionIdError)
      .OnDelete(DeleteBehavior.Cascade);



    modelBuilder.Entity<ForecastDescription>()
      .Property(f => f.isLogTransformed)
      .HasColumnType("bit");

  }
}