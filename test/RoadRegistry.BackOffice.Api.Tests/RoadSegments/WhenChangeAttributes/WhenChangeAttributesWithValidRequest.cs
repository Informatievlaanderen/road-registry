namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithValidRequest : WhenChangeAttributesWithValidRequest<WhenChangeAttributesWithValidRequestFixture>
{
    public WhenChangeAttributesWithValidRequest(WhenChangeAttributesWithValidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }
}