namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline;

using Fixtures;
using Abstractions;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidAccessRestriction : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidAccessRestrictionFixture>
{
    public WhenCreateOutlineWithInvalidAccessRestriction(WhenCreateOutlineWithInvalidAccessRestrictionFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorMessage => "Toegangsbeperking is foutief";
}
