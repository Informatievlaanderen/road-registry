namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using AutoFixture;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;

public class InvalidAantalRijstrokenTests : ChangeDynamicAttributesTestBase
{
    [Fact]
    public async Task VanPositie_VanPositieVerplicht()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = null,
                        TotPositie = 10,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieVerplicht");
    }

    [Fact]
    public async Task VanPositie_VanPositieNulOntbreekt()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 1,
                        TotPositie = 10,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieNulOntbreekt");
    }

    [Fact]
    public async Task VanPositie_VanPositieNietCorrect()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = -1,
                        TotPositie = 20,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieNietCorrect");
    }

    [Fact]
    public async Task TotPositie_TotPositieVerplicht()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = null,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieVerplicht");
    }

    [Fact]
    public async Task TotPositie_TotPositieNietCorrect()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = -1,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietCorrect");
    }

    [Fact]
    public async Task TotPositie_TotPositieNietGelijkAanVolgendeVanPositie()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 1,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietGelijkAanVolgendeVanPositie");
    }

    [Fact]
    public async Task TotPositie_TotPositieKleinerOfGelijkAanVanPositie()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 945,
                        TotPositie = 45,
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieKleinerOfGelijkAanVanPositie");
    }

    [Fact]
    public async Task Aantal_AantalVerplicht()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = null,
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
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
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = aantal.ToString(),
                        Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "AantalNietCorrect");
    }

    [Fact]
    public async Task Richting_RichtingVerplicht()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
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
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = richting
                    }
                }
            }
        }, "RichtingNietCorrect");
    }
}
