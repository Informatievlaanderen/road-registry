namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadNodes.WhenChangeRoadNodeAttributesV2;

using System;
using System.Linq;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadNodes.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadNodes;

public class ChangeRoadNodeAttributesV2Tests : V2ReadEndpointTestBase
{
    private const int MaximumRoadNodeCount = 1000;

    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadNodesController _controller;
    private ChangeRoadNodeAttributesV2SqsRequest? _capturedSqsRequest;

    public ChangeRoadNodeAttributesV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadNodeAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeRoadNodeAttributesV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadNodesController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(ChangeRoadNodeAttributesV2Parameters parameters)
    {
        return _controller.ChangeRoadNodeAttributesV2(parameters, CancellationToken.None);
    }

    private static ChangeRoadNodeAttributeV2Parameters Grensknoop(bool grensknoop, params int[] roadNodeIds)
    {
        return new ChangeRoadNodeAttributeV2Parameters
        {
            Wegknopen = roadNodeIds,
            Grensknoop = grensknoop
        };
    }

    private static async Task<ValidationFailure> ActAndExpectSingleFailure(Func<Task<IActionResult>> act)
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(act);
        return ex.Errors.Should().ContainSingle().Which;
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeRoadNodeAttributesV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // What the endpoint is for: the nodes and the value they should take reach the queue unchanged.
    [Fact]
    public async Task WhenTheRequestIsValid_ThenItIsQueued()
    {
        var result = await Act([Grensknoop(true, 1, 2), Grensknoop(false, 3)]);

        result.Should().BeOfType<AcceptedResult>();

        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest!.Groups.Should().HaveCount(2);
        _capturedSqsRequest.Groups[0].RoadNodeIds.Select(x => x.ToInt32()).Should().BeEquivalentTo([1, 2]);
        _capturedSqsRequest.Groups[0].Grensknoop.Should().BeTrue();
        _capturedSqsRequest.Groups[1].RoadNodeIds.Select(x => x.ToInt32()).Should().BeEquivalentTo([3]);
        _capturedSqsRequest.Groups[1].Grensknoop.Should().BeFalse();
    }

    // VAL-1
    [Fact]
    public async Task WhenTheBodyIsAnEmptyArray_ThenItIsInvalidJson()
    {
        var failure = await ActAndExpectSingleFailure(() => Act([]));

        failure.ErrorMessage.Should().Be("Ongeldige JSON.");
        VerifyNothingWasQueued();
    }

    // VAL-3
    [Fact]
    public async Task WhenTheRoadNodesAreMissing_ThenTheyAreRequired()
    {
        var failure = await ActAndExpectSingleFailure(() => Act([new ChangeRoadNodeAttributeV2Parameters { Grensknoop = true }]));

        failure.ErrorMessage.Should().Be("De parameter 'wegknopen' is verplicht.");
        VerifyNothingWasQueued();
    }

    // VAL-3: leaving 'grensknoop' out is not the same as asking for false - the caller has to say which it is.
    [Fact]
    public async Task WhenGrensknoopIsMissing_ThenItIsRequired()
    {
        var failure = await ActAndExpectSingleFailure(() => Act([new ChangeRoadNodeAttributeV2Parameters { Wegknopen = [1] }]));

        failure.ErrorMessage.Should().Be("De parameter 'grensknoop' is verplicht.");
        VerifyNothingWasQueued();
    }

    // VAL-2
    [Fact]
    public async Task WhenTheSameRoadNodeIsNamedInTwoGroups_ThenTheParameterWasGivenTwice()
    {
        var failure = await ActAndExpectSingleFailure(() => Act([Grensknoop(true, 1, 2), Grensknoop(false, 3, 2)]));

        failure.ErrorMessage.Should().Be("De parameter 'grensknoop' werd meermaals meegegeven voor wegknoop 2.");
        VerifyNothingWasQueued();
    }

    // VAL-4
    [Fact]
    public async Task WhenARoadNodeIdIsNotAnIdentifier_ThenItDoesNotExist()
    {
        var failure = await ActAndExpectSingleFailure(() => Act([Grensknoop(true, -1)]));

        failure.ErrorMessage.Should().Be("De wegknoop -1 bestaat niet.");
        VerifyNothingWasQueued();
    }

    // VAL-7
    [Fact]
    public async Task WhenMoreThanTheMaximumNumberOfRoadNodesIsGiven_ThenTheCeilingIsReported()
    {
        var roadNodeIds = Enumerable.Range(1, MaximumRoadNodeCount + 1).ToArray();

        var failure = await ActAndExpectSingleFailure(() => Act([Grensknoop(true, roadNodeIds)]));

        failure.ErrorMessage.Should().Be($"Het maximaal toegestane aantal objecten waarvoor deze aanpassing mag gebeuren ({MaximumRoadNodeCount}) werd overschreden.");
        VerifyNothingWasQueued();
    }

    // The ceiling counts the distinct identifiers, so a request that only passes it by naming the same node in two
    // groups is a VAL-2 failure rather than a request that is too big.
    [Fact]
    public async Task WhenTheMaximumIsOnlyExceededByDuplicates_ThenTheCeilingIsNotReported()
    {
        var roadNodeIds = Enumerable.Range(1, MaximumRoadNodeCount).ToArray();

        var ex = await Assert.ThrowsAsync<ValidationException>(() => Act([Grensknoop(true, roadNodeIds), Grensknoop(false, roadNodeIds)]));

        ex.Errors.Should().NotContain(x => x.ErrorMessage.Contains("maximaal toegestane aantal"));
        VerifyNothingWasQueued();
    }
}
