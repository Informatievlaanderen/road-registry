namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidAccessRestriction : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidAccessRestrictionFixture>
{
    public WhenCreateOutlineWithInvalidAccessRestriction(WhenCreateOutlineWithInvalidAccessRestrictionFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "ToegangsbeperkingNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Toegangsbeperking is foutief";
}
