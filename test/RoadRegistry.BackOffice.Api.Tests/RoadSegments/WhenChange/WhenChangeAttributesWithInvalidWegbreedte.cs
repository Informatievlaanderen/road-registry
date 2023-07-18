namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChange;

using Abstractions;
using Api.RoadSegments.Change;
using AutoFixture;
using Fixtures;
using Xunit.Abstractions;

public class WhenChangeAttributesWithInvalidWegbreedte : WhenChangeWithInvalidRequest<WhenChangeWithInvalidRequestFixture>
{
    public WhenChangeAttributesWithInvalidWegbreedte(WhenChangeWithInvalidRequestFixture fixture, ITestOutputHelper outputHelper) : base(fixture, outputHelper)
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = null,
                        TotPositie = 10,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
                    }
                }
            }
        }, "VanPositieVerplicht");
    }

    [Fact]
    public async Task VanPositie_VanPositieNulOntbreekt()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 1,
                        TotPositie = 10,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
                    }
                }
            }
        }, "VanPositieNulOntbreekt");
    }

    [Fact]
    public async Task VanPositie_VanPositieNietCorrect()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
                    },
                    new()
                    {
                        VanPositie = -1,
                        TotPositie = 20,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = null,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = -1,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 1,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
                    },
                    new()
                    {
                        VanPositie = 1.00100001M,
                        TotPositie = 3,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
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
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 945,
                        TotPositie = 45,
                        Breedte = Fixture.ObjectProvider.Create<RoadSegmentWidth>()
                    }
                }
            }
        }, "TotPositieKleinerOfGelijkAanVanPositie");
    }

    [Fact]
    public async Task Breedte_BreedteVerplicht()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
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
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsParameters
        {
            new()
            {
                WegsegmentId = Fixture.TestData.Segment1Added.Id,
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new()
                    {
                        VanPositie = 0,
                        TotPositie = 10,
                        Breedte = breedte
                    }
                }
            }
        }, "BreedteNietCorrect");
    }
}
