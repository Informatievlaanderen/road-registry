namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;

public class WhenChangeOutlineGeometryWithInvalidGeometrySridFixture : WhenChangeOutlineGeometryWithValidRequestFixture
{
    public WhenChangeOutlineGeometryWithInvalidGeometrySridFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
        : base(mediator, editorContext, roadSegmentRepository)
    {
    }

    protected override PostChangeOutlineGeometryParameters CreateRequest()
    {
        return base.CreateRequest() with
        {
            MiddellijnGeometrie = @"<gml:MultiLineString srsName=""https://www.opengis.net/def/crs/EPSG/0/10000"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:lineStringMember srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList>
</gml:lineStringMember>
</gml:MultiLineString>"
        };
    }
}
