namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Abstractions.Fixtures;
using Editor.Schema;
using MediatR;

public class WhenChangeAttributesWithInvalidRequestFixture : WhenChangeAttributesFixture
{
    public WhenChangeAttributesWithInvalidRequestFixture(IMediator mediator, EditorContext editorContext)
        : base(mediator, editorContext)
    {
    }
}