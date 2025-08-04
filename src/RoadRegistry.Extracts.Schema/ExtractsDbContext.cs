namespace RoadRegistry.Extracts.Schema;

using System.Reflection;
using BackOffice;
using Microsoft.EntityFrameworkCore;

public class ExtractsDbContext : DbContext
{
    public ExtractsDbContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public ExtractsDbContext(DbContextOptions<ExtractsDbContext> options)
        : base(options)
    {
    }

    public DbSet<ExtractRequest> ExtractRequests { get; set; }
    public DbSet<ExtractDownload> ExtractDownloads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().GetTypeInfo().Assembly);
    }
}
