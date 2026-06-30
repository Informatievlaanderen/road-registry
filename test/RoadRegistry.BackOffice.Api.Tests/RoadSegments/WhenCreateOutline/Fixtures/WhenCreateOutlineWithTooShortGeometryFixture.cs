namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;

public class WhenCreateOutlineWithTooShortGeometryFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithTooShortGeometryFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        return base.CreateRequest() with
        {
            MiddellijnGeometrie = @"<gml:LineString srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2"">
<gml:posList>217368 181577 217368 181578</gml:posList>
</gml:LineString>"
        };
    }
}
