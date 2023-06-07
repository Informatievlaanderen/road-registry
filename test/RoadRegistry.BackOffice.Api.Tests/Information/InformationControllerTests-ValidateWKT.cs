namespace RoadRegistry.BackOffice.Api.Tests.Information;

using Api.Information;

public partial class InformationControllerTests
{
    [Fact]
    public async Task When_wkt_should_be_valid_and_above_maximum()
    {
        var wkt = "MultiPolygon (((64699.86540096173121128 218247.15990484040230513, 91541.66608652254217304 211821.38593515567481518, 91541.66608652254217304 211821.38593515567481518, 91541.66608652254217304 211821.38593515567481518, 91523.78514424958848394 195503.55442933458834887, 88364.38692880698363297 166590.4106055349111557, 50936.49097712300135754 171639.109682597219944, 36050.97635232988977805 199580.85535924322903156, 64699.86540096173121128 218247.15990484040230513)))";
        var result = await Controller.ValidateWKT(new ValidateWktContourRequestBody { Contour = wkt }, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.True(result.IsLargerThanMaximumArea);
    }

    [Fact]
    public async Task When_wkt_should_be_valid_and_allowed()
    {
        var wkt = "MULTIPOLYGON(((188635 163942,188635 163943,188636 163943,188636 163942,188635 163942)))";
        var result = await Controller.ValidateWKT(new ValidateWktContourRequestBody { Contour = wkt }, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.False(result.IsLargerThanMaximumArea);
    }
}