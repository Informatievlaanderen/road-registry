namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithTooShortGeometry : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithTooShortGeometryFixture>
{
    public WhenChangeOutlineGeometryWithTooShortGeometry(WhenChangeOutlineGeometryWithTooShortGeometryFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieKorterDanMinimum";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie heeft niet de minimale lengte van 2 meter.";
}
