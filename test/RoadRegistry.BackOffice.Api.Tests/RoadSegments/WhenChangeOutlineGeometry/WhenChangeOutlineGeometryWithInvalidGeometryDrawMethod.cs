namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry;

using Abstractions;
using Fixtures;

public class WhenChangeOutlineGeometryWithInvalidGeometryDrawMethod : WhenChangeOutlineGeometryWithInvalidRequest<WhenChangeOutlineGeometryWithInvalidGeometryDrawMethodFixture>
{
    public WhenChangeOutlineGeometryWithInvalidGeometryDrawMethod(WhenChangeOutlineGeometryWithInvalidGeometryDrawMethodFixture fixture) : base(fixture)
    {
    }

    protected override string ExpectedErrorCode => "GeometriemethodeNietIngeschetst";
    protected override string ExpectedErrorMessagePrefix => "De geometriemethode van dit wegsegment komt niet overeen met 'ingeschetst'.";
}
