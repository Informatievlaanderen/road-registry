namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidAantalRijstrokenRichting : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidAantalRijstrokenRichtingFixture>
{
    public WhenCreateOutlineWithInvalidAantalRijstrokenRichting(WhenCreateOutlineWithInvalidAantalRijstrokenRichtingFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "AantalRijstrokenRichtingNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Aantal rijstroken richting is foutief.";
}
