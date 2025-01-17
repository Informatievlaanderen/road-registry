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
FROM [RoadRegistryBackOfficeEventHost].[EventProcessorPosition] with (nolock)
UNION ALL
SELECT [Name]
    ,[Position]
    ,null as DesiredState
    ,null as DesiredStateChangedAt
    ,null as ErrorMessage
FROM [RoadRegistryBackOfficeExtractHost].[EventProcessorPosition] with (nolock)
UNION ALL
SELECT q.Name
    ,(CASE WHEN q.MaxStreamVersion = q.Version THEN q.MaxPosition ELSE q.Position END) as Position
    ,null as DesiredState
    ,null as DesiredStateChangedAt
    ,null as ErrorMessage
FROM (
SELECT cpp.[Name]
    ,cpp.[Version]
    ,m.[Position]
    ,s.Version as MaxStreamVersion
    ,(SELECT MAX(Position) FROM [RoadRegistry].Messages with (nolock)) as MaxPosition
FROM [RoadRegistryBackOfficeCommandHost].[CommandProcessorPosition] cpp with (nolock)
JOIN [RoadRegistry].Streams s with (nolock) ON cpp.Name = s.IdOriginal
JOIN [RoadRegistry].Messages m with (nolock) ON s.[IdInternal] = m.[StreamIdInternal] AND cpp.[Version] = m.[StreamVersion]
) q
")
                .HasKey(x => x.Name);
        }
    }
}
