namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Abstractions.Fixtures;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using RoadRegistry.Editor.Schema;

public class WhenCreateOutlineWithInvalidLaneDirectionFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidLaneDirectionFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        var request = base.CreateRequest();
        request.AantalRijstroken.Richting = "";
        return request;
    }
}
