namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Fixtures;

using Abstractions.Fixtures;
using Api.RoadSegments;
using Editor.Schema;
using MediatR;

public class WhenDeleteOutlineWithValidRequestFixture : WhenDeleteOutlineFixture
{
    public WhenDeleteOutlineWithValidRequestFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
        : base(mediator, editorContext, roadSegmentRepository)
    {
    }
}
