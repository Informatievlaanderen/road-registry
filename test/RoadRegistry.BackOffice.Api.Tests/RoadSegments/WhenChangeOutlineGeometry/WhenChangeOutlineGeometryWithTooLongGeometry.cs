namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;

public class WhenChangeOutlineGeometryWithTooLongGeometry : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithTooLongGeometryFixture>
{
    public WhenChangeOutlineGeometryWithTooLongGeometry(WhenChangeOutlineGeometryWithTooLongGeometryFixture fixture) : base(fixture)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieTeLang";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie zijn lengte is groter of gelijk dan 100000 meter.";
}
