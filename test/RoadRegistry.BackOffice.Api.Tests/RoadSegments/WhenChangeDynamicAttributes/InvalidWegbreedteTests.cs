namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using AutoFixture;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;

public class InvalidWegbreedteTests : ChangeDynamicAttributesTestBase
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = null,
                        TotPositie = 10,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 1,
                        TotPositie = 10,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = -1,
                        TotPositie = 20,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = null,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = -1,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 1,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 945,
                        TotPositie = 45,
                        Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString()
                    }
                }
            }
        }, "TotPositieKleinerOfGelijkAanVanPositie");
    }

    [Fact]
    public async Task Breedte_BreedteVerplicht()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = null
                    }
                }
            }
        }, "BreedteVerplicht");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(51)]
    public async Task Breedte_BreedteNietCorrect(int breedte)
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = breedte.ToString()
                    }
                }
            }
        }, "BreedteNietCorrect");
    }
}
