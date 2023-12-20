namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Abstractions.Fixtures;
using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using Editor.Schema;
using MediatR;

public class WhenChangeAttributesWithValidRequestFixture : WhenChangeAttributesFixture
{
    public WhenChangeAttributesWithValidRequestFixture(IMediator mediator, EditorContext editorContext)
        : base(mediator, editorContext)
    {
    }

    protected override ChangeRoadSegmentAttributesParameters CreateRequest()
    {
        return new ChangeRoadSegmentAttributesParameters
        {
            new()
            {
                Wegbeheerder = TestData.ChangedByOrganization,
                Wegsegmenten = new[] { TestData.Segment1Added.Id }
            },
            new()
            {
                MorfologischeWegklasse = ObjectProvider.Create<RoadSegmentMorphology>().ToDutchString(),
                Wegsegmenten = new[] { TestData.Segment1Added.Id }
            },
            new()
            {
                Wegcategorie = ObjectProvider.Create<RoadSegmentCategory>().ToDutchString(),
                Wegsegmenten = new[] { TestData.Segment1Added.Id }
            },
            new()
            {
                Wegsegmentstatus = ObjectProvider.Create<RoadSegmentStatus>().ToDutchString(),
                Wegsegmenten = new[] { TestData.Segment1Added.Id }
            },
            new()
            {
                Toegangsbeperking = ObjectProvider.Create<RoadSegmentAccessRestriction>().ToDutchString(),
                Wegsegmenten = new[] { TestData.Segment1Added.Id }
            }
        };
    }
}
