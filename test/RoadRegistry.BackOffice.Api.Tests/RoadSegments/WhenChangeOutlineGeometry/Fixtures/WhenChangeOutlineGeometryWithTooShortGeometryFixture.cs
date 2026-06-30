namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;

public class WhenChangeOutlineGeometryWithTooShortGeometryFixture : WhenChangeOutlineGeometryWithValidRequestFixture
{
    public WhenChangeOutlineGeometryWithTooShortGeometryFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
        : base(mediator, editorContext, roadSegmentRepository)
    {
    }

    protected override PostChangeOutlineGeometryParameters CreateRequest()
    {
        return base.CreateRequest() with
        {
            MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368 181577 217368 181578</gml:posList>
</gml:LineString>"
        };
    }
}
