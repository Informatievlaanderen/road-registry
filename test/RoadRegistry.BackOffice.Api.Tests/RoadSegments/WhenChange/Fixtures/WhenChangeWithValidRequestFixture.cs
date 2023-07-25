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
                        Aantal = "niet gekend",
                        Richting = "niet gekend"
                    },
                    new()
                    {
                        VanPositie = 10.0001M,
                        TotPositie = 20,
                        Aantal = "niet van toepassing",
                        Richting = "niet van toepassing"
                    },
                    new()
                    {
                        VanPositie = 20.0001M,
                        TotPositie = 30,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                },
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = "niet gekend",
                    },
                    new()
                    {
                        VanPositie = 10.0001M,
                        TotPositie = 20,
                        Breedte = "niet van toepassing"
                    },
                    new()
                    {
                        VanPositie = 20.0001M,
                        TotPositie = 30,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
                    }
                },
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Type = "niet gekend",
                    },
                    new()
                    {
                        VanPositie = 10.0001M,
                        TotPositie = 20,
                        Type = "niet van toepassing"
                    },
                    new()
                    {
                        VanPositie = 20.0001M,
                        TotPositie = 30,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    }
                }
            }
        };
    }
}
