namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes;

using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.RoadSegments.V1.ChangeDynamicAttributes;

public class ChangeDynamicAttributesTests : ChangeDynamicAttributesTestBase
{
    [Fact]
    public async Task ValidRequest_AcceptedResult()
    {
        await GivenRoadNetwork();

        var result = await GetResultAsync(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = TestData.Segment1Added.Id,
                AantalRijstroken = new ChangeLaneAttributeParameters[]
                {
                    new() { VanPositie = 0, TotPositie = 10, Aantal = "niet gekend", Richting = "niet gekend" },
                    new() { VanPositie = 10.0001M, TotPositie = 20, Aantal = "niet van toepassing", Richting = "niet van toepassing" },
                    new() { VanPositie = 20.0001M, TotPositie = 30, Aantal = ObjectProvider.Create<RoadSegmentLaneCount>().ToDutchString(), Richting = ObjectProvider.Create<RoadSegmentLaneDirection>().ToDutchString() }
                },
                Wegbreedte = new ChangeWidthAttributeParameters[]
                {
                    new() { VanPositie = 0, TotPositie = 10, Breedte = "niet gekend" },
                    new() { VanPositie = 10.0001M, TotPositie = 20, Breedte = "niet van toepassing" },
                    new() { VanPositie = 20.0001M, TotPositie = 30, Breedte = ObjectProvider.Create<RoadSegmentWidth>().ToDutchString() }
                },
                Wegverharding = new ChangeSurfaceAttributeParameters[]
                {
                    new() { VanPositie = 0, TotPositie = 10, Type = "niet gekend" },
                    new() { VanPositie = 10.0001M, TotPositie = 20, Type = "niet van toepassing" },
                    new() { VanPositie = 20.0001M, TotPositie = 30, Type = ObjectProvider.Create<RoadSegmentSurfaceType>().ToDutchString() }
                }
            }
        });

        Assert.IsType<AcceptedResult>(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task WegsegmentId_JsonInvalid(int? wegsegmentId)
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = wegsegmentId,
                Wegbreedte = new[] { new ChangeWidthAttributeParameters { VanPositie = 0, TotPositie = 10, Breedte = "1" } }
            }
        }, "JsonInvalid");
    }

    [Fact]
    public async Task WegsegmentId_NotFound()
    {
        await GivenRoadNetwork();

        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new()
            {
                WegsegmentId = int.MaxValue,
                Wegbreedte = new[] { new ChangeWidthAttributeParameters { VanPositie = 0, TotPositie = 10, Breedte = "1" } }
            }
        }, "NotFound");
    }

    [Fact]
    public async Task Attribuut_JsonInvalid()
    {
        await GivenRoadNetwork();

        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters
        {
            new() { WegsegmentId = TestData.Segment1Added.Id }
        }, "JsonInvalid");
    }

    [Fact]
    public async Task NullRequest_NotFound()
    {
        await ItShouldHaveExpectedError(null, "NotFound");
    }

    [Fact]
    public async Task NullItem_JsonInvalid()
    {
        await ItShouldHaveExpectedError(new ChangeRoadSegmentsDynamicAttributesParameters { null }, "JsonInvalid");
    }
}
