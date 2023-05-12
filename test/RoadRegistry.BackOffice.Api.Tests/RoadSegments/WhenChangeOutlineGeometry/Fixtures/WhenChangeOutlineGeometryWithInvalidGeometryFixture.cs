namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using Api.RoadSegments;
using Editor.Schema;
using MediatR;

public class WhenChangeOutlineGeometryWithInvalidGeometryFixture : WhenChangeOutlineGeometryWithValidRequestFixture
{
    public WhenChangeOutlineGeometryWithInvalidGeometryFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
    }

    protected override PostChangeOutlineGeometryParameters CreateRequest()
    {
        return base.CreateRequest() with { MiddellijnGeometrie = "abc" };
    }
}