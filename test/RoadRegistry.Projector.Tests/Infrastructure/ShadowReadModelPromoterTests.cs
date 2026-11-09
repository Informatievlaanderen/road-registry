namespace RoadRegistry.Projector.Tests.Infrastructure;

using RoadRegistry.Projector.Infrastructure;

public class ShadowReadModelPromoterTests
{
    [Fact]
    public void A_shadow_that_is_still_switched_on_is_not_promoted()
    {
        var decision = ShadowReadModelPromoter.Decide(shadowEnabled: true, shadowTableCount: 10, shadowPosition: 100, shadowSequence: 100);

        Assert.False(decision.Promote);
        Assert.Contains("Shadow:Enabled", decision.Reason);
    }

    [Fact]
    public void A_shadow_without_tables_is_not_promoted()
    {
        var decision = ShadowReadModelPromoter.Decide(shadowEnabled: false, shadowTableCount: 0, shadowPosition: null, shadowSequence: 100);

        Assert.False(decision.Promote);
        Assert.Contains("no tables", decision.Reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0L)]
    public void A_shadow_that_has_not_projected_anything_is_not_promoted(long? shadowPosition)
    {
        var decision = ShadowReadModelPromoter.Decide(shadowEnabled: false, shadowTableCount: 10, shadowPosition, shadowSequence: 100);

        Assert.False(decision.Promote);
        Assert.Contains("has not projected anything", decision.Reason);
    }

    [Fact]
    public void A_shadow_without_a_marten_progression_is_not_promoted()
    {
        var decision = ShadowReadModelPromoter.Decide(shadowEnabled: false, shadowTableCount: 10, shadowPosition: 100, shadowSequence: null);

        Assert.False(decision.Promote);
        Assert.Contains("no Marten progression", decision.Reason);
    }

    // How far it got is up to the caller: the live projection catches up on the rest after the swap, even when the
    // shadow is behind it.
    [Theory]
    [InlineData(1L)]
    [InlineData(100L)]
    public void A_switched_off_shadow_with_tables_and_a_position_is_promoted(long shadowSequence)
    {
        var decision = ShadowReadModelPromoter.Decide(shadowEnabled: false, shadowTableCount: 10, shadowPosition: 100, shadowSequence);

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
