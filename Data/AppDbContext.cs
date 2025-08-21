using System.Text.Json;
using ForecastingGas.Data.Entities;
using ForecastingGas.Dto.Requests;
using Microsoft.EntityFrameworkCore;

namespace ForecastingGas.Data;

public class AppDbContext : DbContext
{

    public DbSet<ActualValues> GetActualValues { get; set; }
    public DbSet<DataDescription> DataDescriptions { get; set; }

    public DbSet<ForecastDescription> GetForecastDescriptions { get; set; }
    public DbSet<ForecastValues> GetForecastValues { get; set; }

    public DbSet<ErrorValues> GetErrorValues { get; set; }

    // public DbSet<NeuroGASForecastDescription> NeuroGASForecastDescriptions { get; set; }
    // public DbSet<NeuroGASForecastValues> NeuroGASForecastValues { get; set; }

    // public DbSet<NeuroGASWBDescription> NeuroGASWBDescriptions { get; set; }
    // public DbSet<Neurons> Neurons { get; set; }
    // public DbSet<Weights> Weights { get; set; }


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

        // modelBuilder.Entity<NeuroGASForecastValues>()
        //       .HasOne(v => v.GetNeuroGASForecastDescription)
        //       .WithMany(d => d.GetNeuroGASForecastValues)
        //       .HasForeignKey(v => v.NeuroGASForecastDescriptionID)
        //       .OnDelete(DeleteBehavior.Cascade);

        // // WBDescription → Neurons
        // modelBuilder.Entity<Neurons>()
        //     .HasOne(n => n.GetNeuroGASWBDescription)
        //     .WithMany(d => d.Neurons)
        //     .HasForeignKey(n => n.NeuroGASWBDescriptionID)
        //     .OnDelete(DeleteBehavior.Cascade);

        // // Neuron → Weights
        // modelBuilder.Entity<Weights>()
        //     .HasOne(w => w.GetNeurons)
        //     .WithMany(n => n.Weights)
        //     .HasForeignKey(w => w.NeuronsId)
        //     .OnDelete(DeleteBehavior.Cascade);
    }
}