namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes.Fixtures;

using Abstractions.Fixtures;
using MediatR;
using RoadRegistry.Editor.Schema;

public class WhenChangeDynamicAttributesWithInvalidRequestFixture : WhenChangeDynamicAttributesFixture
{
    public WhenChangeDynamicAttributesWithInvalidRequestFixture(IMediator mediator, EditorContext editorContext)
        : base(mediator, editorContext)
    {
    }
}