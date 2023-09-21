namespace RoadRegistry.Sync.OrganizationRegistry;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.BackOffice;

public class OrganizationConsumerContext : RunnerDbContext<OrganizationConsumerContext>
{
    public OrganizationConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public OrganizationConsumerContext(DbContextOptions<OrganizationConsumerContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellknownSchemas.OrganizationConsumerSchema;

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
