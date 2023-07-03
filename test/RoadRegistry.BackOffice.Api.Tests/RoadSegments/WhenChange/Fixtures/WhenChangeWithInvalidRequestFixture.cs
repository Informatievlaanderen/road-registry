namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange.Fixtures;

using Abstractions.Fixtures;
using Editor.Schema;
using MediatR;

public class WhenChangeWithInvalidRequestFixture : WhenChangeFixture
{
    public WhenChangeWithInvalidRequestFixture(IMediator mediator, EditorContext editorContext)
        : base(mediator, editorContext)
    {
    }
}