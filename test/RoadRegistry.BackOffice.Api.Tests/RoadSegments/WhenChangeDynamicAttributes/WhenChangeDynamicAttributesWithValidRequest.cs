namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeWithValidRequest : WhenChangeDynamicAttributesWithValidRequest<WhenChangeDynamicAttributesWithValidRequestFixture>
{
    public WhenChangeWithValidRequest(WhenChangeDynamicAttributesWithValidRequestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture, outputHelper)
    {
    }
}