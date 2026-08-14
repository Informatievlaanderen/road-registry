namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenChangeRoadSegmentGeometryDrawMethodV2;

using System.Linq;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;

public class ChangeRoadSegmentGeometryDrawMethodV2Tests : V2ReadEndpointTestBase
{
    private const int MaximumRoadSegmentCount = 1000;

    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;

    public ChangeRoadSegmentGeometryDrawMethodV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentGeometryDrawMethodV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(ChangeRoadSegmentGeometryDrawMethodV2Parameters parameters)
    {
        return _controller.ChangeRoadSegmentGeometryDrawMethodV2(parameters, CancellationToken.None);
    }

    private static ChangeRoadSegmentGeometryDrawMethodV2GroupParameters ChangeToIngemeten(params int[] roadSegmentIds)
    {
        return new ChangeRoadSegmentGeometryDrawMethodV2GroupParameters
        {
            Wegsegmenten = roadSegmentIds,
            Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingemeten.ToDutchString()
        };
    }

    private static int[] RoadSegmentIds(int count, int start = 1)
    {
        return Enumerable.Range(start, count).ToArray();
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeRoadSegmentGeometryDrawMethodV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private ChangeRoadSegmentGeometryDrawMethodV2SqsRequest _capturedSqsRequest;

    private void CaptureSqsRequest()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentGeometryDrawMethodV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeRoadSegmentGeometryDrawMethodV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());
    }

    [Fact]
    public async Task GivenAnEmptyBody_ThenValidationException()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act([]));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Ongeldige JSON.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoRoadSegments_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            new()
            {
                Wegsegmenten = null,
                Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingemeten.ToDutchString()
            }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0].wegsegmenten");
        failure.ErrorMessage.Should().Be("De parameter 'wegsegmenten' is verplicht.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnEmptyArrayOfRoadSegments_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            ChangeToIngemeten()
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0].wegsegmenten");
        failure.ErrorMessage.Should().Be("De parameter 'wegsegmenten' is verplicht.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenNoGeometryDrawMethod_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            new() { Wegsegmenten = [1] }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0].geometriemethode");
        failure.ErrorMessage.Should().Be("De parameter 'geometriemethode' is verplicht.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenAnUnknownGeometryDrawMethod_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            new() { Wegsegmenten = [1], Geometriemethode = "geen geometriemethode" }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[0].geometriemethode");
        failure.ErrorMessage.Should().Be("De parameter 'geometriemethode' heeft een ongeldige waarde.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenTheSameRoadSegmentInTwoGroups_ThenValidationException()
    {
        // The draw method is set as a whole, so the same segment in two groups would silently pick a winner.
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            ChangeToIngemeten(1, 2),
            new()
            {
                Wegsegmenten = [3, 2],
                Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingeschetst.ToDutchString()
            }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        var failure = ex.Errors.Should().ContainSingle().Which;
        failure.PropertyName.Should().Be("[1]");
        failure.ErrorMessage.Should().Be("De parameter 'geometriemethode' werd meermaals meegegeven voor wegsegment 2.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenSeveralInvalidChanges_ThenEveryFailureIsReported()
    {
        // Shape validation collects: the caller gets everything that is wrong with the request in one go.
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            new() { Wegsegmenten = [1] },
            new() { Wegsegmenten = [2], Geometriemethode = "geen geometriemethode" },
            new() { Wegsegmenten = null, Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingemeten.ToDutchString() }
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        ex.Errors.Select(x => x.PropertyName).Should().BeEquivalentTo(["[0].geometriemethode", "[1].geometriemethode", "[2].wegsegmenten"]);
        VerifyNothingWasQueued();
    }

    [Theory]
    [InlineData("ingeschetst")]
    [InlineData("ingemeten")]
    public async Task GivenAValidRequest_ThenTheParsedDrawMethodIsSentAlong(string geometriemethode)
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            new() { Wegsegmenten = [1, 2], Geometriemethode = geometriemethode }
        };
        CaptureSqsRequest();

        var result = await Act(parameters);

        result.Should().BeOfType<AcceptedResult>();
        var group = _capturedSqsRequest.Groups.Should().ContainSingle().Which;
        group.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1), new RoadSegmentId(2)]);
        group.GeometryDrawMethod.Should().Be(RoadSegmentGeometryDrawMethodV2.ParseUsingDutchName(geometriemethode));
    }

    [Fact]
    public async Task GivenTwoGroupsWithDifferentRoadSegments_ThenBothAreSentAlong()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            ChangeToIngemeten(1, 2),
            new()
            {
                Wegsegmenten = [3],
                Geometriemethode = RoadSegmentGeometryDrawMethodV2.Ingeschetst.ToDutchString()
            }
        };
        CaptureSqsRequest();

        var result = await Act(parameters);

        result.Should().BeOfType<AcceptedResult>();
        _capturedSqsRequest.Groups.Should().HaveCount(2);
        _capturedSqsRequest.Groups[0].GeometryDrawMethod.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingemeten);
        _capturedSqsRequest.Groups[1].GeometryDrawMethod.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
    }

    [Fact]
    public async Task GivenTheMaximumAmountOfRoadSegments_ThenAccepted()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            ChangeToIngemeten(RoadSegmentIds(MaximumRoadSegmentCount))
        };

        var result = await Act(parameters);

        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task GivenMoreThanTheMaximumAmountOfRoadSegments_ThenValidationException()
    {
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            ChangeToIngemeten(RoadSegmentIds(MaximumRoadSegmentCount + 1))
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be($"Er kunnen maximaal {MaximumRoadSegmentCount} wegsegmenten gewijzigd worden. Er werden er {MaximumRoadSegmentCount + 1} opgegeven.");
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenMoreThanTheMaximumAmountOfRoadSegmentsSpreadOverMultipleGroups_ThenValidationException()
    {
        // The limit is on the request as a whole, not on a single group.
        var parameters = new ChangeRoadSegmentGeometryDrawMethodV2Parameters
        {
            ChangeToIngemeten(RoadSegmentIds(600)),
            ChangeToIngemeten(RoadSegmentIds(600, start: 601))
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act(parameters));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be($"Er kunnen maximaal {MaximumRoadSegmentCount} wegsegmenten gewijzigd worden. Er werden er 1200 opgegeven.");
        VerifyNothingWasQueued();
    }
}
