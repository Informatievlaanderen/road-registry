namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Fixtures;

using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Abstractions.Fixtures;
using RoadSegments.Parameters;

public class WhenCreateOutlineWithInvalidLaneDirectionFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidLaneDirectionFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostRoadSegmentOutlineParameters CreateRequestParameters()
    {
        var request = base.CreateRequestParameters();
        request.AantalRijstroken.Richting = "";
        return request;
    }
}
