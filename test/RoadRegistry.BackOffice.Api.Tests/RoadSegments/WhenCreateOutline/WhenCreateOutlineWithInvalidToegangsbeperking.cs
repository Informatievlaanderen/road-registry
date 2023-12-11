namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidToegangsbeperking : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidToegangsbeperkingFixture>
{
    public WhenCreateOutlineWithInvalidToegangsbeperking(WhenCreateOutlineWithInvalidToegangsbeperkingFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "ToegangsbeperkingNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Toegangsbeperking is foutief";
}
