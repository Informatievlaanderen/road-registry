namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Fixtures;

using Abstractions.Fixtures;
using Api.RoadSegments;
using AutoFixture;
using Editor.Schema;
using MediatR;
using RoadRegistry.Tests.BackOffice;

public class WhenCreateOutlineWithValidRequestFixture : WhenCreateOutlineFixture
{
    public WhenCreateOutlineWithValidRequestFixture(IMediator mediator, EditorContext editorContext) : base(mediator, editorContext)
    {
        ObjectProvider.CustomizeRoadSegmentOutline();
    }

    protected override PostRoadSegmentOutlineParameters CreateRequest()
    {
        return new PostRoadSegmentOutlineParameters
        {
            MiddellijnGeometrie = GeometryTranslatorTestCases.ValidGmlMultiLineString,
            Wegsegmentstatus = ObjectProvider.Create<RoadSegmentStatus>().ToDutchString(),
            MorfologischeWegklasse = ObjectProvider.Create<RoadSegmentMorphology>().ToDutchString(),
            Toegangsbeperking = ObjectProvider.Create<RoadSegmentAccessRestriction>().ToDutchString(),
            Wegbeheerder = "TEST",
            Wegverharding = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString(),
            Wegbreedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString(),
            AantalRijstroken = new RoadSegmentLaneParameters
            {
                Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
            }
        };
    }
}
