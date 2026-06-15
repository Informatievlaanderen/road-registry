namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.V1;

public class WhenCreateOutlineWithInvalidWegverhardingFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidWegverhardingFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        return base.CreateRequest() with { Wegverharding = "" };
    }
}
