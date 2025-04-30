namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidWegcategorie : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidWegcategorieFixture>
{
    public WhenCreateOutlineWithInvalidWegcategorie(WhenCreateOutlineWithInvalidWegcategorieFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "WegcategorieNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Wegcategorie is foutief.";
}
