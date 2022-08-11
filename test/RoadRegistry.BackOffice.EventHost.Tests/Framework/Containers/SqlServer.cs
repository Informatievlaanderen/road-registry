namespace RoadRegistry.BackOffice.EventHost.Tests.Framework.Containers;

using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Framework;
using RoadRegistry.Framework.Containers;
using RoadRegistry.Product.Schema;

public class SqlServer : ISqlServerDatabase
{
    private readonly ISqlServerDatabase _inner;

    public SqlServer()
    {
        if (Environment.GetEnvironmentVariable("CI") == null)
            _inner = new SqlServerEmbeddedContainer();
        else
            _inner = new SqlServerComposedContainer();

        MemoryStreamManager = new RecyclableMemoryStreamManager();
        StreetNameCache = new StreetNameCacheStub();
    }

    public RecyclableMemoryStreamManager MemoryStreamManager { get; }
    public IStreetNameCache StreetNameCache { get; }

    public Task InitializeAsync()
    {
        return _inner.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return _inner.DisposeAsync();
    }

    public Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
    {
        return _inner.CreateDatabaseAsync();
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
}
