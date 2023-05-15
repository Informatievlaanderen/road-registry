namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using Editor.Schema;
using MediatR;

public class WhenDeleteOutlineWithInvalidIdFixture : WhenDeleteOutlineWithValidRequestFixture
{
    public WhenDeleteOutlineWithInvalidIdFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override int CreateRequest()
    {
        return 0;
    }
}