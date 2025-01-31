namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;

public class WhenChangeOutlineGeometryWithTooShortGeometry : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithTooShortGeometryFixture>
{
    public WhenChangeOutlineGeometryWithTooShortGeometry(WhenChangeOutlineGeometryWithTooShortGeometryFixture fixture) : base(fixture)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieKorterDanMinimum";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie heeft niet de minimale lengte van 2 meter.";
}
