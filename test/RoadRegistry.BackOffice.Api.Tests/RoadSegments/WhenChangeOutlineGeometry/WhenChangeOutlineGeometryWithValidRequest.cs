namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithValidRequest : WhenChangeOutlineGeometryWithValidRequest<WhenChangeOutlineGeometryWithValidRequestFixture>
{
    public WhenChangeOutlineGeometryWithValidRequest(WhenChangeOutlineGeometryWithValidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }
}