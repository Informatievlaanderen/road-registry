namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidLaneCount : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidLaneCountFixture>
{
    public WhenCreateOutlineWithInvalidLaneCount(WhenCreateOutlineWithInvalidLaneCountFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "AantalRijstrokenNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Aantal rijstroken is foutief. '0' is geen geldige waarde.";
}
