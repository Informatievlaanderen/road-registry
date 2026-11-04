namespace RoadRegistry.Projector.Tests.Infrastructure;

using Microsoft.Extensions.Logging;
using RoadRegistry.Projector.Infrastructure;

public class ShadowReadModelPromoterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    public void A_shadow_that_has_not_projected_anything_is_not_promoted(long? shadowPosition)
    {
        var decision = ShadowReadModelPromoter.Decide(shadowPosition, shadowSequence: 100, liveSequence: 50);

        Assert.False(decision.Promote);
        Assert.Equal(LogLevel.Information, decision.Level);
    }

    [Fact]
    public void A_shadow_without_a_marten_progression_is_not_promoted()
    {
        var decision = ShadowReadModelPromoter.Decide(shadowPosition: 100, shadowSequence: null, liveSequence: 50);

        Assert.False(decision.Promote);
    }

    [Fact]
    public void A_shadow_behind_the_live_projection_is_not_promoted()
    {
        var decision = ShadowReadModelPromoter.Decide(shadowPosition: 100, shadowSequence: 99, liveSequence: 100);

        Assert.False(decision.Promote);
        Assert.Equal(LogLevel.Warning, decision.Level);
    }

    // Not at the store position is fine: the live projection catches up on the rest after the swap.
    [Theory]
    [InlineData(100L)]
    [InlineData(150L)]
    public void A_shadow_level_with_or_ahead_of_the_live_projection_is_promoted(long shadowSequence)
    {
        var decision = ShadowReadModelPromoter.Decide(shadowPosition: 100, shadowSequence, liveSequence: 100);

        Assert.True(decision.Promote);
    }

    [Fact]
    public void A_shadow_is_promoted_where_the_live_projection_never_ran()
    {
        var decision = ShadowReadModelPromoter.Decide(shadowPosition: 100, shadowSequence: 100, liveSequence: null);

        Assert.True(decision.Promote);
    }

    [Fact]
    public void A_spatial_index_is_scripted_with_its_tessellation()
    {
        var index = new ShadowReadModelPromoter.IndexDefinition(
            "road", "Wegknopen", "SIDX_Wegknopen_GEOMETRIE", Type: 4, IsUnique: false, FilterDefinition: null,
            new ShadowReadModelPromoter.SpatialTessellation("GEOMETRY_GRID", 522200, 653000, 758900, 744100, "MEDIUM", "MEDIUM", "MEDIUM", "MEDIUM", 16));
        index.KeyColumns.Add(new ShadowReadModelPromoter.IndexColumn("GEOMETRIE", IsDescending: false));

        Assert.Equal(
            "CREATE SPATIAL INDEX [SIDX_Wegknopen_GEOMETRIE] ON [roadTemp].[Wegknopen] ([GEOMETRIE]) USING GEOMETRY_GRID"
            + " WITH (BOUNDING_BOX = (522200, 653000, 758900, 744100), GRIDS = (LEVEL_1 = MEDIUM, LEVEL_2 = MEDIUM, LEVEL_3 = MEDIUM, LEVEL_4 = MEDIUM), CELLS_PER_OBJECT = 16);",
            index.Script("roadTemp"));
    }

    [Fact]
    public void A_rowstore_index_is_scripted_with_its_keys_includes_and_filter()
    {
        var index = new ShadowReadModelPromoter.IndexDefinition(
            "wms", "Wegsegmenten", "IX_Wegsegmenten_TempId", Type: 1, IsUnique: true, FilterDefinition: "([TempId]>(0))", Tessellation: null);
        index.KeyColumns.Add(new ShadowReadModelPromoter.IndexColumn("TempId", IsDescending: false));
        index.KeyColumns.Add(new ShadowReadModelPromoter.IndexColumn("Versie", IsDescending: true));
        index.IncludedColumns.Add("Status");

        Assert.Equal(
            "CREATE UNIQUE CLUSTERED INDEX [IX_Wegsegmenten_TempId] ON [wms].[Wegsegmenten] ([TempId] ASC, [Versie] DESC) INCLUDE ([Status]) WHERE ([TempId]>(0));",
            index.Script("wms"));
    }

    [Theory]
    [InlineData("GRANT", null, "GRANT SELECT ON [road].[Wegknopen] TO [reader];")]
    [InlineData("GRANT_WITH_GRANT_OPTION", null, "GRANT SELECT ON [road].[Wegknopen] TO [reader] WITH GRANT OPTION;")]
    [InlineData("DENY", "GEOMETRIE", "DENY SELECT ON [road].[Wegknopen] ([GEOMETRIE]) TO [reader];")]
    public void A_permission_is_scripted_as_it_was_granted(string state, string? column, string expected)
    {
        var permission = new ShadowReadModelPromoter.PermissionDefinition(state, "SELECT", "road", "Wegknopen", column, "reader");

        Assert.Equal(expected, permission.Script());
    }
}
