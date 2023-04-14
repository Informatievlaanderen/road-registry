namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenDeleteOutlineWithInvalidId : WhenDeleteOutlineWithInvalidRequest<WhenDeleteOutlineWithInvalidIdFixture>
{
    public WhenDeleteOutlineWithInvalidId(WhenDeleteOutlineWithInvalidIdFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "IncorrectObjectId";
    protected override string ExpectedErrorMessagePrefix => $"De waarde '{Fixture.Request}' is ongeldig.";
}
