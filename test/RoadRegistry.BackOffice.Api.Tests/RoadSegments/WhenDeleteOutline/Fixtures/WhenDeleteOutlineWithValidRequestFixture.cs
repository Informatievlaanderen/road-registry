using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using Editor.Schema;
using MediatR;

public class WhenDeleteOutlineWithValidRequestFixture : WhenDeleteOutlineFixture
{
    public WhenDeleteOutlineWithValidRequestFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }
}
