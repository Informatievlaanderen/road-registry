namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Sync.MunicipalityRegistry;
using RoadRegistry.Tests.Framework.Projections;

public class DbContextBuilder
{
    public EditorContext CreateEditorContext()
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new MemoryEditorContext(options);
    }

    public MunicipalityEventConsumerContext CreateMunicipalityEventConsumerContext()
    {
        var options = new DbContextOptionsBuilder<MunicipalityEventConsumerContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new MunicipalityEventConsumerContext(options);
    }

    public ExtractsDbContext CreateExtractsDbContext()
    {
        return new FakeExtractsDbContextFactory().CreateDbContext();
    }
}
