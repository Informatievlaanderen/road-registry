namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineWithValidRequest : WhenCreateOutlineWithValidRequest<WhenCreateOutlineWithValidRequestFixture>
{
    public WhenCreateOutlineWithValidRequest(WhenCreateOutlineWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }
}