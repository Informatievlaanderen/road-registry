namespace RoadRegistry.StreetNameConsumer.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class StreetNameConsumerContext : RunnerDbContext<StreetNameConsumerContext>
{
    public StreetNameConsumerContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public StreetNameConsumerContext(DbContextOptions<StreetNameConsumerContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellknownSchemas.StreetNameConsumerSchema;

    public DbSet<StreetNameConsumerItem> StreetNames { get; set; }
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
