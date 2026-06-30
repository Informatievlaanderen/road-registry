namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenCreateOutlineV2WithValidRequest : WhenCreateOutlineV2WithValidRequest<WhenCreateOutlineV2WithValidRequestFixture>
{
    public WhenCreateOutlineV2WithValidRequest(WhenCreateOutlineV2WithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }
}
