namespace RoadRegistry.Sync.OrganizationRegistry;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

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

    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    
    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplyConfiguration(new ProcessedMessageConfiguration(ProjectionStateSchema));
    }
}
