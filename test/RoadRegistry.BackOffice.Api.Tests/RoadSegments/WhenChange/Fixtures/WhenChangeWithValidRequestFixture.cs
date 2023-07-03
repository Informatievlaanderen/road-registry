namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange.Fixtures;

using Abstractions.Fixtures;
using Api.RoadSegments.Change;
using AutoFixture;
using Editor.Schema;
using MediatR;

public class WhenChangeWithValidRequestFixture : WhenChangeFixture
{
    public WhenChangeWithValidRequestFixture(IMediator mediator, EditorContext editorContext)
        : base(mediator, editorContext)
    {
    }

    protected override ChangeRoadSegmentsParameters CreateRequest()
    {
        return new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 10.0001M,
                        TotPositie = 20,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                },
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>()
                    },
                    new()
                    {
                        VanPositie = 10.0001M,
                        TotPositie = 20,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>()
                    }
                },
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 10.0001M,
                        TotPositie = 20,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    }
                }
            }
        };
    }
}
