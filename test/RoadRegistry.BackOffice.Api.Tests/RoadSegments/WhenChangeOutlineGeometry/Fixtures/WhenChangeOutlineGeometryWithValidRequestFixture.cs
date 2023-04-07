using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Tests.BackOffice;

public class WhenChangeOutlineGeometryWithValidRequestFixture : WhenChangeOutlineGeometryFixture
{
    public WhenChangeOutlineGeometryWithValidRequestFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
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
