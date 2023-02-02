namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline;

using Fixtures;
using Abstractions;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidStatus : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidStatusFixture>
{
    public WhenCreateOutlineWithInvalidStatus(WhenCreateOutlineWithInvalidStatusFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorMessage => "Wegsegment status is foutief";
}
