namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using AutoFixture;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;

public class InvalidWegverhardingTests : ChangeDynamicAttributesTestBase
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = null,
                        TotPositie = 10,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 1,
                        TotPositie = 10,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                        VanPositie = -1,
                        TotPositie = 20,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = null,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = -1,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 1,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 945,
                        TotPositie = 45,
                        Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    }
                }
            }
        }, "TotPositieKleinerOfGelijkAanVanPositie");
    }

    [Fact]
    public async Task Type_TypeVerplicht()
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Type = null
                    }
                }
            }
        }, "TypeVerplicht");
    }

    [Theory]
    [InlineData("")]
    [InlineData("$abc$")]
    public async Task Type_TypeNietCorrect(string type)
    {
        await GivenRoadNetwork();
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Type = type
                    }
                }
            }
        }, "TypeNietCorrect");
    }
}
