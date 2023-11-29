namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;

public class WhenDeleteOutlineWithInvalidIdFixture : WhenDeleteOutlineWithValidRequestFixture
{
    public WhenDeleteOutlineWithInvalidIdFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
        : base(mediator, editorContext, roadSegmentRepository)
    {
    }

    protected override int CreateRequest()
    {
        return 0;
    }
}
