using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using Api.RoadSegments;
using AutoFixture;
using Editor.Schema;
using MediatR;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;

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
                Attribuut = ChangeRoadSegmentAttribute.Wegbeheerder.ToString(),
                Attribuutwaarde = TestData.ChangedByOrganization,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.MorfologischeWegklasse.ToString(),
                Attribuutwaarde = ObjectProvider.Create<RoadSegmentMorphology>().Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegcategorie.ToString(),
                Attribuutwaarde = ObjectProvider.Create<RoadSegmentCategory>().Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.WegsegmentStatus.ToString(),
                Attribuutwaarde = ObjectProvider.Create<RoadSegmentStatus>().Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Toegangsbeperking.ToString(),
                Attribuutwaarde = ObjectProvider.Create<RoadSegmentAccessRestriction>().Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            }
        };
    }
}
