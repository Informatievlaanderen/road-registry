namespace RoadRegistry.Syndication.ProjectionHost.Tests.Framework.Containers;

using BackOffice;
using BackOffice.Abstractions;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Product.Schema;
using RoadRegistry.Tests.Framework.Containers;

public class SqlServer : ISqlServerDatabase
{
    private readonly ISqlServerDatabase _inner;

    public SqlServer()
    {
        _inner = SqlServerDatabaseFactory.Create(RoadRegistryAssembly.SyndicationProjectionHost);

        MemoryStreamManager = new RecyclableMemoryStreamManager();
        StreetNameCache = new FakeStreetNameCache();
    }

    public RecyclableMemoryStreamManager MemoryStreamManager { get; }
    public IStreetNameCache StreetNameCache { get; }

    public Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
    {
        return _inner.CreateDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return _inner.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        return _inner.InitializeAsync();
    }

    public async Task<EditorContext> CreateEditorContextAsync(SqlConnectionStringBuilder builder)
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseSqlServer(builder.ConnectionString,
                dbContextOptionsBuilder => dbContextOptionsBuilder.UseNetTopologySuite())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new EditorContext(options);
        await context.Database.MigrateAsync();
        return context;
    }

    public async Task<EditorContext> CreateEmptyEditorContextAsync(SqlConnectionStringBuilder builder)
    {
        var context = await CreateEditorContextAsync(builder);

        context.Organizations.RemoveRange(context.Organizations);
        context.RoadNodes.RemoveRange(context.RoadNodes);
        context.RoadSegments.RemoveRange(context.RoadSegments);
        context.RoadSegmentEuropeanRoadAttributes.RemoveRange(context.RoadSegmentEuropeanRoadAttributes);
        context.RoadSegmentNationalRoadAttributes.RemoveRange(context.RoadSegmentNationalRoadAttributes);
        context.RoadSegmentNumberedRoadAttributes.RemoveRange(context.RoadSegmentNumberedRoadAttributes);
        context.RoadSegmentLaneAttributes.RemoveRange(context.RoadSegmentLaneAttributes);
        context.RoadSegmentWidthAttributes.RemoveRange(context.RoadSegmentWidthAttributes);
        context.RoadSegmentSurfaceAttributes.RemoveRange(context.RoadSegmentSurfaceAttributes);
        context.GradeSeparatedJunctions.RemoveRange(context.GradeSeparatedJunctions);
        context.RoadNetworkInfo.RemoveRange(context.RoadNetworkInfo);
        context.ProjectionStates.RemoveRange(context.ProjectionStates);
        await context.SaveChangesAsync();

        return context;
    }

    public async Task<ProductContext> CreateEmptyProductContextAsync(SqlConnectionStringBuilder builder)
    {
        var context = await CreateProductContextAsync(builder);

        context.Organizations.RemoveRange(context.Organizations);
        context.RoadNodes.RemoveRange(context.RoadNodes);
        context.RoadSegments.RemoveRange(context.RoadSegments);
        context.RoadSegmentEuropeanRoadAttributes.RemoveRange(context.RoadSegmentEuropeanRoadAttributes);
        context.RoadSegmentNationalRoadAttributes.RemoveRange(context.RoadSegmentNationalRoadAttributes);
        context.RoadSegmentNumberedRoadAttributes.RemoveRange(context.RoadSegmentNumberedRoadAttributes);
        context.RoadSegmentLaneAttributes.RemoveRange(context.RoadSegmentLaneAttributes);
        context.RoadSegmentWidthAttributes.RemoveRange(context.RoadSegmentWidthAttributes);
        context.RoadSegmentSurfaceAttributes.RemoveRange(context.RoadSegmentSurfaceAttributes);
        context.GradeSeparatedJunctions.RemoveRange(context.GradeSeparatedJunctions);
        context.RoadNetworkInfo.RemoveRange(context.RoadNetworkInfo);
        context.ProjectionStates.RemoveRange(context.ProjectionStates);
        await context.SaveChangesAsync();

        return context;
    }

    public async Task<ProductContext> CreateProductContextAsync(SqlConnectionStringBuilder builder)
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseSqlServer(builder.ConnectionString)
            .EnableSensitiveDataLogging()
            .Options;

        var context = new ProductContext(options);
        await context.Database.MigrateAsync();
        return context;
    }
}
