namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Fixtures;

using Api.RoadSegmentsOutline;
using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegmentsOutline.Parameters;
using RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Abstractions.Fixtures;

public class WhenCreateOutlineWithInvalidStatusFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidStatusFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostRoadSegmentOutlineParameters CreateRequestParameters()
    {
        return base.CreateRequestParameters() with { Wegsegmentstatus = "" };
    }
}
