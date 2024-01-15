namespace RoadRegistry.Hosts;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BackOfficeProcessorDbContext : RunnerDbContext<BackOfficeProcessorDbContext>
{
    public BackOfficeProcessorDbContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public BackOfficeProcessorDbContext(DbContextOptions<BackOfficeProcessorDbContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.EventSchema;

    public DbSet<ProjectionStateItem> ProcessorPositions { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProcessorPositionConfiguration());
    }

    private sealed class ProcessorPositionConfiguration : IEntityTypeConfiguration<ProjectionStateItem>
    {
        public void Configure(EntityTypeBuilder<ProjectionStateItem> b)
        {
            b
                .ToSqlQuery(@"
SELECT [Name]
    ,[Position]
    ,null as DesiredState
    ,null as DesiredStateChangedAt
    ,null as ErrorMessage
FROM [RoadRegistryBackOfficeEventHost].[EventProcessorPosition]
UNION ALL
SELECT [Name]
    ,[Position]
    ,null as DesiredState
    ,null as DesiredStateChangedAt
    ,null as ErrorMessage
FROM [RoadRegistryBackOfficeExtractHost].[EventProcessorPosition]
UNION ALL
SELECT q.Name
    ,(q.MaxPosition - (q.MaxStreamVersion - q.Version)) as Position
    ,null as DesiredState
    ,null as DesiredStateChangedAt
    ,null as ErrorMessage
FROM (
SELECT cpp.[Name]
    ,cpp.[Version]
    ,s.Version as MaxStreamVersion
    ,s.Position as MaxStreamPosition
    ,(SELECT MAX(Position) FROM [RoadRegistry].Messages) as MaxPosition
FROM [RoadRegistryBackOfficeCommandHost].[CommandProcessorPosition] cpp
JOIN [RoadRegistry].Streams s ON cpp.Name = s.IdOriginal
) q
")
                .HasKey(x => x.Name);
        }
    }
}
