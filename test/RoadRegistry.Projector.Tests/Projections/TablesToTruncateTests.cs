namespace RoadRegistry.Projector.Tests.Projections;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoadRegistry.Projector.Projections;
using RoadRegistry.WmsWfsV1Inwinning;
using RoadRegistry.WmsWfsV1Inwinning.Records;
using RoadRegistry.WmsWfsV2.Schema;

public class TablesToTruncateTests
{
    // Only the model is needed: building it does not connect.
    private const string ConnectionString = "Server=localhost;Database=unused;Trusted_Connection=True;";

    [Fact]
    public void WmsWfsV1InwinningTruncatesOnlyItsOwnListsAndNeverTheV1Tables()
    {
        using var context = new WmsWfsV1InwinningContext(new DbContextOptionsBuilder<WmsWfsV1InwinningContext>()
            .UseSqlServer(ConnectionString)
            .Options);

        var tables = ProjectionsController.GetTablesToTruncate(context, excludeEntity: null);

        tables.Should().BeEquivalentTo("[RoadRegistry].[CompletedRoadSegments]", "[RoadRegistry].[CompletedRoadNodes]");
    }

    [Fact]
    public void AnExcludedEntityIsNotTruncated()
    {
        using var context = new WmsWfsV1InwinningContext(new DbContextOptionsBuilder<WmsWfsV1InwinningContext>()
            .UseSqlServer(ConnectionString)
            .Options);

        var tables = ProjectionsController.GetTablesToTruncate(context, clrType => clrType == typeof(CompletedRoadNodeRecord));

        tables.Should().BeEquivalentTo("[RoadRegistry].[CompletedRoadSegments]");
    }

    [Fact]
    public void WmsWfsV2TruncatesItsTablesButNotItsProjectionStates()
    {
        using var context = new WmsWfsV2Context(new DbContextOptionsBuilder<WmsWfsV2Context>()
            .UseSqlServer(ConnectionString, o => o.UseNetTopologySuite())
            .Options);

        var tables = ProjectionsController.GetTablesToTruncate(context, excludeEntity: null);

        tables.Should().Contain("[road].[Wegsegmenten]")
            .And.NotContain(table => table.EndsWith("[ProjectionStates]"));
    }
}
