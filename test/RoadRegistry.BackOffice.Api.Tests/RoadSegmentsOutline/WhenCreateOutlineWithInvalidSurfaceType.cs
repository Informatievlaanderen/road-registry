namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidSurfaceType : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidSurfaceTypeFixture>
{
    public WhenCreateOutlineWithInvalidSurfaceType(WhenCreateOutlineWithInvalidSurfaceTypeFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorMessage => "Wegverharding is foutief";
}
