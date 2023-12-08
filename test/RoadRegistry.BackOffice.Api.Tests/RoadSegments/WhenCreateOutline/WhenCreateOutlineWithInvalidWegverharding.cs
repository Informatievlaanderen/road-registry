namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithInvalidWegverharding : WhenCreateOutlineWithInvalidRequest<WhenCreateOutlineWithInvalidSurfaceTypeFixture>
{
    public WhenCreateOutlineWithInvalidWegverharding(WhenCreateOutlineWithInvalidSurfaceTypeFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "WegverhardingNietCorrect";
    protected override string ExpectedErrorMessagePrefix => "Wegverharding is foutief";
}
