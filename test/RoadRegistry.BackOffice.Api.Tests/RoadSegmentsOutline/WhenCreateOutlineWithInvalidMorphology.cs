namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline;

using Fixtures;
using Abstractions;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidMorphology : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidMorphologyFixture>
{
    public WhenCreateOutlineWithInvalidMorphology(WhenCreateOutlineWithInvalidMorphologyFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorMessage => "Morfologische wegklasse is foutief";
}
