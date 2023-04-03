namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenDeleteOutlineWithValidRequest : WhenDeleteOutlineWithValidRequest<WhenDeleteOutlineWithValidRequestFixture>
{
    public WhenDeleteOutlineWithValidRequest(WhenDeleteOutlineWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }
}
