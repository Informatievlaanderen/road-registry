namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using Abstractions;
using Api.RoadSegments.ChangeDynamicAttributes;
using AutoFixture;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeDynamicAttributesWithInvalidAantalRijstroken : WhenChangeDynamicAttributesWithInvalidRequest<WhenChangeDynamicAttributesWithInvalidRequestFixture>
{
    public WhenChangeDynamicAttributesWithInvalidAantalRijstroken(WhenChangeDynamicAttributesWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
    {
    }

    [Fact]
    public async Task VanPositie_VanPositieVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieVerplicht");
    }

    [Fact]
    public async Task VanPositie_VanPositieNulOntbreekt()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 1,
                        TotPositie = 10,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieNulOntbreekt");
    }

    [Fact]
    public async Task VanPositie_VanPositieNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = -1,
                        TotPositie = 20,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "VanPositieNietCorrect");
    }

    [Fact]
    public async Task TotPositie_TotPositieVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieVerplicht");
    }

    [Fact]
    public async Task TotPositie_TotPositieNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietCorrect");
    }

    [Fact]
    public async Task TotPositie_TotPositieNietGelijkAanVolgendeVanPositie()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietGelijkAanVolgendeVanPositie");
    }

    [Fact]
    public async Task TotPositie_TotPositieKleinerOfGelijkAanVanPositie()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 945,
                        TotPositie = 45,
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "TotPositieKleinerOfGelijkAanVanPositie");
    }

    [Fact]
    public async Task Aantal_AantalVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = aantal.ToString(),
                        Richting = Fixture.ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString()
                    }
                }
            }
        }, "AantalNietCorrect");
    }

    [Fact]
    public async Task Richting_RichtingVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
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
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
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
                        Aantal = Fixture.ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(),
                        Richting = richting
                    }
                }
            }
        }, "RichtingNietCorrect");
    }
}
