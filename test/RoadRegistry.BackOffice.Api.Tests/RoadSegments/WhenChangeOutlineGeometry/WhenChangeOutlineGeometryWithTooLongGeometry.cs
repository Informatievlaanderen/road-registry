namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithTooLongGeometry : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithTooLongGeometryFixture>
{
    public WhenChangeOutlineGeometryWithTooLongGeometry(WhenChangeOutlineGeometryWithTooLongGeometryFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieTeLang";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie zijn lengte is groter of gelijk dan 100000 meter.";
}
