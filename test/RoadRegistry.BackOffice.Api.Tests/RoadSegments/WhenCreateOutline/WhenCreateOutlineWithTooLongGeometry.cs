namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithTooLongGeometry : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithTooLongGeometryFixture>
{
    public WhenCreateOutlineWithTooLongGeometry(WhenCreateOutlineWithTooLongGeometryFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "MiddellijnGeometrieTeLang";
    protected override string ExpectedErrorMessagePrefix => "De opgegeven geometrie zijn lengte is groter of gelijk dan 100000 meter.";
}
