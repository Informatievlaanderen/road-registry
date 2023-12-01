namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Fixtures;

using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Api.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class WhenChangeOutlineGeometryWithInvalidGeometryDrawMethodFixture : WhenChangeOutlineGeometryWithValidRequestFixture
{
    public WhenChangeOutlineGeometryWithInvalidGeometryDrawMethodFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
        : base(mediator, editorContext, roadSegmentRepository)
    {
        TestData = new RoadNetworkTestData(fixture => { fixture.CustomizeRoadSegmentGeometryDrawMethod(); }).CopyCustomizationsTo(ObjectProvider);
    }
}
