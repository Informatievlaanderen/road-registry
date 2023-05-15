namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidMorphology : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidMorphologyFixture>
{
    public WhenCreateOutlineWithInvalidMorphology(WhenCreateOutlineWithInvalidMorphologyFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "MorfologischeWegklasseNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Morfologische wegklasse is foutief";
}