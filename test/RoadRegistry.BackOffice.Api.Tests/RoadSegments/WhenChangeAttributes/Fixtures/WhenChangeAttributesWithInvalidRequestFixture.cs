using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using MediatR;
using Editor.Schema;

public class WhenChangeAttributesWithInvalidRequestFixture : WhenChangeAttributesFixture
{
    public WhenChangeAttributesWithInvalidRequestFixture(IMediator mediator, EditorContext editorContext)
        : base(mediator, editorContext)
    {
    }
}
