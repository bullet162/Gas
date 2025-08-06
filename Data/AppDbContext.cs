using ForecastingGas.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data;

public class AppDbContext : DbContext
{

    public DbSet<ActualValues> GetActualValues { get; set; }
    public DbSet<DataDescription> DataDescriptions { get; set; }
    public DbSet<ForecastDescription> GetForecastDescriptions { get; set; }
    public DbSet<ForecastValues> GetForecastValues { get; set; }
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

    }
}