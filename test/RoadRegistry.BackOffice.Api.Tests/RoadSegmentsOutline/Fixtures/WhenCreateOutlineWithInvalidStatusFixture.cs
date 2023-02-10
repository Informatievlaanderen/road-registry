namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Fixtures;

using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Abstractions.Fixtures;
using RoadSegments.Parameters;

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
