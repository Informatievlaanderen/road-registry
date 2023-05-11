using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Api.RoadSegments;
using AutoFixture;
using MediatR;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Tests.BackOffice;

public class WhenCreateOutlineWithValidRequestFixture : WhenCreateOutlineFixture
{
    public WhenCreateOutlineWithValidRequestFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
        ObjectProvider.CustomizeRoadSegmentOutlineStatus();
        ObjectProvider.CustomizeRoadSegmentOutlineMorphology();
        ObjectProvider.CustomizeRoadSegmentOutlineSurfaceType();
        ObjectProvider.CustomizeRoadSegmentOutlineLaneCount();
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        return new PostRoadSegmentOutlineParameters
        {
            MiddellijnGeometrie = GeometryTranslatorTestCases.ValidGmlMultiLineString,
            Wegsegmentstatus = ObjectProvider.Create<RoadSegmentStatus>().Translation.Name,
            MorfologischeWegklasse = ObjectProvider.Create<RoadSegmentMorphology>().Translation.Name,
            Toegangsbeperking = ObjectProvider.Create<RoadSegmentAccessRestriction>().Translation.Name,
            Wegbeheerder = "TEST",
            Wegverharding = ObjectProvider.Create<RoadSegmentSurfaceType>().Translation.Name,
            Wegbreedte = 5,
            AantalRijstroken = new RoadSegmentLaneParameters
            {
                Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToInt32(),
                Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().Translation.Name
            }
        };
    }
}
