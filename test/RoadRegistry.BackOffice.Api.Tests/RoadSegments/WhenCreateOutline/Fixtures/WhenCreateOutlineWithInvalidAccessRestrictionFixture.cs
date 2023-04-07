namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Abstractions.Fixtures;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using RoadRegistry.Editor.Schema;

public class WhenCreateOutlineWithInvalidAccessRestrictionFixture : WhenCreateOutlineWithValidRequestFixture
{
    public WhenCreateOutlineWithInvalidAccessRestrictionFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        return base.CreateRequest() with { Toegangsbeperking = "" };
    }
}
