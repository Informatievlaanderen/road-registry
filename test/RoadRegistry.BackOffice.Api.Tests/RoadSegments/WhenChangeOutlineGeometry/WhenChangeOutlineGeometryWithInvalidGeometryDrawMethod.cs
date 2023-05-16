namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeOutlineGeometryWithInvalidGeometryDrawMethod : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithInvalidGeometryDrawMethodFixture>
{
    public WhenChangeOutlineGeometryWithInvalidGeometryDrawMethod(WhenChangeOutlineGeometryWithInvalidGeometryDrawMethodFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    protected override string ExpectedErrorCode => "GeometriemethodeNietIngeschetst";
    protected override string ExpectedErrorMessagePrefix => "De geometriemethode van dit wegsegment komt niet overeen met 'ingeschetst'.";
}