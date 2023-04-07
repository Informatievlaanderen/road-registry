namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidStatus : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidStatusFixture>
{
    public WhenCreateOutlineWithInvalidStatus(WhenCreateOutlineWithInvalidStatusFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "WegsegmentStatusNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Wegsegment status is foutief";
}
