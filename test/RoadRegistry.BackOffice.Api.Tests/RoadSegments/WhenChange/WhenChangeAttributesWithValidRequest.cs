namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeWithValidRequest : WhenChangeWithValidRequest<WhenChangeWithValidRequestFixture>
{
    public WhenChangeWithValidRequest(WhenChangeWithValidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }
}