namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange;

using Abstractions;
using Api.RoadSegments.Change;
using AutoFixture;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidAantalRijstroken : WhenChangeWithInvalidRequest<WhenChangeWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidAantalRijstroken(WhenChangeWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task VanPositie_VanPositieVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = null,
                        TotPositie = 10,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieVerplicht");
    }

    [Fact]
    public async Task VanPositie_VanPositieNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = -1,
                        TotPositie = 10,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieNietCorrect");
    }

    [Fact]
    public async Task TotPositie_TotPositieVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = null,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieVerplicht");
    }

    [Fact]
    public async Task TotPositie_TotPositieNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = -1,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietCorrect");
    }

    [Fact]
    public async Task TotPositie_TotPositieNietGelijkAanVolgendeVanPositie()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 1,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietGelijkAanVolgendeVanPositie");
    }

    [Fact]
    public async Task Aantal_AantalVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Aantal = null,
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "AantalVerplicht");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public async Task Aantal_AantalNietCorrect(int aantal)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Aantal = aantal,
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "AantalNietCorrect");
    }

    [Fact]
    public async Task Richting_RichtingVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = null
                    }
                }
            }
        }, "RichtingVerplicht");
    }

    [Theory]
    [InlineData("")]
    [InlineData("$abc")]
    public async Task Richting_RichtingNietCorrect(string richting)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>(),
                        Richting = richting
                    }
                }
            }
        }, "RichtingNietCorrect");
    }
}
