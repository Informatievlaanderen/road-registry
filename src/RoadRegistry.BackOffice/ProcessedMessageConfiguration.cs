namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public const string TableName = "ProcessedMessages";

    private readonly string _schemaName;

    public ProcessedMessageConfiguration(string schemaName)
    {
        _schemaName = schemaName;
    }

    public void Configure(EntityTypeBuilder<ProcessedMessage> entityBuilder)
    {
        entityBuilder
            .ToTable(TableName, _schemaName)
            .HasKey(p => p.IdempotenceKey)
            .IsClustered();

        entityBuilder
            .Property(p => p.IdempotenceKey)
            .HasMaxLength(128); //SHA512 length

        entityBuilder
            .Property(p => p.DateProcessed);

        entityBuilder
            .HasIndex(p => p.DateProcessed);
    }
}
