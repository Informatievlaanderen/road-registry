namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange;

using Abstractions;
using Api.RoadSegments.Change;
using AutoFixture;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegverharding : WhenChangeWithInvalidRequest<WhenChangeWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegverharding(WhenChangeWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = null,
                        TotPositie = 10,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = -1,
                        TotPositie = 10,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = null,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = -1,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
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
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 1,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    }
                }
            }
        }, "TotPositieNietGelijkAanVolgendeVanPositie");
    }

    [Fact]
    public async Task TotPositie_TotPositieKleinerOfGelijkAanVanPositie()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 945,
                        TotPositie = 45,
                        Type = Fixture.ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString()
                    }
                }
            }
        }, "TotPositieKleinerOfGelijkAanVanPositie");
    }

    [Fact]
    public async Task Type_TypeVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
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
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
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
