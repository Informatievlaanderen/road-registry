namespace RoadRegistry.Projections.Tests.Projections;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.BackOffice;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;

// The shadow read models are the same context type mapped to a second schema, and what has to hold is that the two
// models coexist: were they ever to share one, the first schema to build it would be the one every later instance
// writes to - silently, and into the live read model. These pin that from the outside, whichever of the two mechanisms
// keeping them apart (a service provider per schema, a model cache key per schema) is doing the work.
//
// The contexts are configured for SQL Server but never opened: building a model does not connect.
public class TempSchemaReadModelTests
{
    private static WmsWfsV2Context WmsWfsV2(string? schema = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WmsWfsV2Context>()
            .UseSqlServer("Server=not-connected;Database=none;", o => o.UseNetTopologySuite());

        return new WmsWfsV2Context(schema is null ? optionsBuilder.Options : optionsBuilder.UseSchema(schema).Options);
    }

    private static PbsContext Pbs(string? schema = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PbsContext>()
            .UseSqlServer("Server=not-connected;Database=none;", o => o.UseNetTopologySuite());

        return new PbsContext(schema is null ? optionsBuilder.Options : optionsBuilder.UseSchema(schema).Options);
    }

    [Fact]
    public void TheShadowContextMapsEveryTableToTheShadowSchema()
    {
        using var live = WmsWfsV2();
        using var shadow = WmsWfsV2(WellKnownSchemas.WmsWfsV2TempSchema);

        // Built after the live one, so a model shared across schemas would hand it the live schema.
        var schemas = shadow.Model.GetEntityTypes()
            .Where(x => x.GetTableName() is not null)
            .Select(x => x.GetSchema())
            .Distinct();

        schemas.Should().Equal(WellKnownSchemas.WmsWfsV2TempSchema);
        live.Model.GetEntityTypes()
            .Where(x => x.GetTableName() is not null)
            .Select(x => x.GetSchema())
            .Distinct()
            .Should().Equal(WellKnownSchemas.WmsWfsV2Schema);
    }

    [Fact]
    public void TheShadowContextKeepsItsProjectionStateRowInTheShadowSchema()
    {
        using var shadow = Pbs(WellKnownSchemas.PbsTempSchema);

        shadow.ProjectionStateSchema.Should().Be(WellKnownSchemas.PbsTempSchema);
        shadow.Model.FindEntityType(typeof(Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates.ProjectionStateItem))!
            .GetSchema().Should().Be(WellKnownSchemas.PbsTempSchema);
    }

    [Fact]
    public void ThePbsShadowContextMapsEveryTableToTheShadowSchema()
    {
        using var live = Pbs();
        using var shadow = Pbs(WellKnownSchemas.PbsTempSchema);

        shadow.Model.GetEntityTypes()
            .Where(x => x.GetTableName() is not null)
            .Select(x => x.GetSchema())
            .Distinct()
            .Should().Equal(WellKnownSchemas.PbsTempSchema);
        live.Model.GetEntityTypes()
            .Where(x => x.GetTableName() is not null)
            .Select(x => x.GetSchema())
            .Distinct()
            .Should().Equal(WellKnownSchemas.PbsSchema);
    }


    // How the live read model is registered, and the shape that first broke it: EF's DbContextFactory builds an
    // activator for the context type and refuses a type with more than one constructor it could use.
    [Fact]
    public void TheLiveContextIsStillResolvableThroughAddDbContextFactory()
    {
        var services = new ServiceCollection()
            .AddDbContextFactory<PbsContext>(options => options
                .UseSqlServer("Server=not-connected;Database=none;", o => o.UseNetTopologySuite()))
            .BuildServiceProvider();

        using var context = services.GetRequiredService<IDbContextFactory<PbsContext>>().CreateDbContext();

        context.Schema.Should().Be(WellKnownSchemas.PbsSchema);
    }

    // Marten keys a shard and its progressions by the projection's name, and the read model keys its position row by
    // it, so the shadow being a type of its own is what keeps the two rebuilds apart.
    [Fact]
    public void TheShadowProjectionsCarryANameOfTheirOwn()
    {
        typeof(RoadNetworkChangesWmsWfsV2TempProjection).Name
            .Should().NotBe(typeof(RoadNetworkChangesWmsWfsV2Projection).Name);
        typeof(RoadNetworkChangesPbsTempProjection).Name
            .Should().NotBe(typeof(RoadNetworkChangesPbsProjection).Name);

        WellKnownProjectionStateNames.RoadNetworkChangesWmsWfsV2TempProjection
            .Should().Be($"{nameof(RoadNetworkChangesWmsWfsV2TempProjection)}:All");
        WellKnownProjectionStateNames.RoadNetworkChangesPbsTempProjection
            .Should().Be($"{nameof(RoadNetworkChangesPbsTempProjection)}:All");
    }
}
