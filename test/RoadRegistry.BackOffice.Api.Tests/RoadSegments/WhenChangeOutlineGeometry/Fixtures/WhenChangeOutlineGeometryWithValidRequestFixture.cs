namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using Abstractions.Fixtures;
using Api.RoadSegments;
using Editor.Schema;
using MediatR;
using RoadRegistry.Tests.BackOffice;

public class WhenChangeOutlineGeometryWithValidRequestFixture : WhenChangeOutlineGeometryFixture
{
    public WhenChangeOutlineGeometryWithValidRequestFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
    	: base(mediator, editorContext, roadSegmentRepository)
    {
    }

    protected override PostChangeOutlineGeometryParameters CreateRequest()
    {
        return new PostChangeOutlineGeometryParameters
        {
            MiddellijnGeometrie = GeometryTranslatorTestCases.ValidGmlMultiLineString
        };
    }
}
