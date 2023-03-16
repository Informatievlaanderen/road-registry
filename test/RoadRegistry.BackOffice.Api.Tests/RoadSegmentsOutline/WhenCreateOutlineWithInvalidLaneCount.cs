namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline;

using Fixtures;
using Abstractions;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidLaneCount : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidLaneCountFixture>
{
    public WhenCreateOutlineWithInvalidLaneCount(WhenCreateOutlineWithInvalidLaneCountFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "AantalRijstrokenVerplicht";
    protected override string ExpectedErrorMessagePrefix => "Aantal rijstroken moet groter dan nul zijn.";
}
