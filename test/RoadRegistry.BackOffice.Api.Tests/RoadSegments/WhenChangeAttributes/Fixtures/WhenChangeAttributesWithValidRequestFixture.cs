using RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Fixtures;

using MediatR;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using Editor.Schema;

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
                Attribuutwaarde = RoadSegmentMorphology.Parse(TestData.Segment1Added.Morphology).Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Wegcategorie.ToString(),
                Attribuutwaarde = RoadSegmentCategory.Parse(TestData.Segment1Added.Category).Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.WegsegmentStatus.ToString(),
                Attribuutwaarde = RoadSegmentStatus.Parse(TestData.Segment1Added.Status).Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            },
            new()
            {
                Attribuut = ChangeRoadSegmentAttribute.Toegangsbeperking.ToString(),
                Attribuutwaarde = RoadSegmentAccessRestriction.Parse(TestData.Segment1Added.AccessRestriction).Translation.Name,
                Wegsegmenten = new []{ TestData.Segment1Added.Id }
            }
        };
    }
}
