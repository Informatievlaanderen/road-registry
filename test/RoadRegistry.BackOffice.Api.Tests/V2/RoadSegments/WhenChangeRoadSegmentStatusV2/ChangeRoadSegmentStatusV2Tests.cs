namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenChangeRoadSegmentStatusV2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoFixture;
using BackOffice.Handlers.Sqs.RoadSegments.V2;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice;

// Every status change endpoint does the same thing and differs in nothing but the transition it names, so they are
// tested as one: each endpoint is exercised through the controller action it is actually reached by.
public class ChangeRoadSegmentStatusV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;
    private readonly RoadNetworkTestDataV2 TestData = new();

    public ChangeRoadSegmentStatusV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentStatusV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    // The action on the controller for each transition, so a transition that is not reachable through an endpoint of
    // its own fails here rather than going unnoticed.
    private IReadOnlyDictionary<RoadSegmentStatusChange, Func<int, Task<IActionResult>>> Endpoints =>
        new Dictionary<RoadSegmentStatusChange, Func<int, Task<IActionResult>>>
        {
            [RoadSegmentStatusChange.PlannedToRealized] = id => _controller.ChangeRoadSegmentFromPlannedToRealizedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.OutOfUseToRealized] = id => _controller.ChangeRoadSegmentFromOutOfUseToRealizedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.RealizedToOutOfUse] = id => _controller.ChangeRoadSegmentFromRealizedToOutOfUseV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.RealizedToHistorized] = id => _controller.ChangeRoadSegmentFromRealizedToHistorizedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.OutOfUseToHistorized] = id => _controller.ChangeRoadSegmentFromOutOfUseToHistorizedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.RealizedToPlanned] = id => _controller.CorrectRoadSegmentFromRealizedToPlannedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.NotRealizedToPlanned] = id => _controller.CorrectRoadSegmentFromNotRealizedToPlannedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.HistorizedToRealized] = id => _controller.CorrectRoadSegmentFromHistorizedToRealizedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None),
            [RoadSegmentStatusChange.HistorizedToOutOfUse] = id => _controller.CorrectRoadSegmentFromHistorizedToOutOfUseV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None)
        };

    public static TheoryData<string> AllChanges()
    {
        var data = new TheoryData<string>();
        foreach (var statusChange in RoadSegmentStatusChange.All)
        {
            data.Add(statusChange.Name);
        }
        return data;
    }

    private Task<IActionResult> Act(RoadSegmentStatusChange statusChange, int id)
    {
        return Endpoints[statusChange](id);
    }

    private int SeedRoadSegment(bool isRemoved = false)
    {
        var e = TestData.Segment1Added;
        var readItem = BuildReadItem(e);
        readItem.IsRemoved = isRemoved;
        Seed(readItem);
        return (int)e.RoadSegmentId;
    }

    private void GiveCallerScopes(params string[] scopes)
    {
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(scopes.Select(x => new Claim(AcmIdmClaimTypes.Scope, x)), "Test"));
    }

    private ChangeRoadSegmentStatusV2SqsRequest _capturedSqsRequest;

    private void CaptureSqsRequest()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<ChangeRoadSegmentStatusV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (ChangeRoadSegmentStatusV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<ChangeRoadSegmentStatusV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void EveryStatusChangeHasAnEndpointOfItsOwn()
    {
        Endpoints.Keys.Should().BeEquivalentTo(RoadSegmentStatusChange.All);
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public async Task GivenAnExistingRoadSegment_ThenAcceptedAndTheTransitionIsSentAlong(string change)
    {
        // The action carries nothing else: everything it does follows from the network around the segment.
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var id = SeedRoadSegment();
        CaptureSqsRequest();

        var result = await Act(statusChange, id);

        result.Should().BeOfType<AcceptedResult>();
        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest.RoadSegmentId.Should().Be(new RoadSegmentId(id));
        _capturedSqsRequest.StatusChange.Should().Be(statusChange);
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public async Task GivenNonExistingRoadSegment_ThenNotFound(string change)
    {
        // VAL-2
        SeedRoadSegment();

        var result = await Act(RoadSegmentStatusChange.Parse(change), 999);

        result.Should().BeOfType<NotFoundResult>();
        VerifyNothingWasQueued();
    }

    [Theory]
    [MemberData(nameof(AllChanges))]
    public async Task GivenRemovedRoadSegment_ThenGone(string change)
    {
        // VAL-3
        var id = SeedRoadSegment(isRemoved: true);

        var result = await Act(RoadSegmentStatusChange.Parse(change), id);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
        VerifyNothingWasQueued();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GivenInvalidRoadSegmentId_ThenValidationException(int id)
    {
        // VAL-1 is checked before anything is looked up, so an unusable id never reaches the store.
        await Assert.ThrowsAsync<ValidationException>(() => Act(RoadSegmentStatusChange.PlannedToRealized, id));
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenCallerWithIngemetenScope_ThenMeasuredRoadSegmentsMayBeModified()
    {
        var id = SeedRoadSegment();
        GiveCallerScopes(Scopes.DvWrIngemetenWegBeheer);
        CaptureSqsRequest();

        await Act(RoadSegmentStatusChange.PlannedToRealized, id);

        _capturedSqsRequest.MayModifyMeasuredRoadSegments.Should().BeTrue();
    }

    [Fact]
    public async Task GivenCallerWithOnlyGeschetsteWegScope_ThenMeasuredRoadSegmentsMayNotBeModified()
    {
        var id = SeedRoadSegment();
        GiveCallerScopes(Scopes.DvWrGeschetsteWegBeheer);
        CaptureSqsRequest();

        await Act(RoadSegmentStatusChange.PlannedToRealized, id);

        _capturedSqsRequest.MayModifyMeasuredRoadSegments.Should().BeFalse();
    }

    [Fact]
    public async Task GivenScopesInASingleSpaceSeparatedClaim_ThenTheIngemetenScopeIsStillRecognised()
    {
        // Depending on the authentication scheme the scopes arrive as one claim per scope or as a single claim
        // holding all of them.
        var id = SeedRoadSegment();
        GiveCallerScopes($"{Scopes.DvWrGeschetsteWegBeheer} {Scopes.DvWrIngemetenWegBeheer}");
        CaptureSqsRequest();

        await Act(RoadSegmentStatusChange.PlannedToRealized, id);

        _capturedSqsRequest.MayModifyMeasuredRoadSegments.Should().BeTrue();
    }

    private static RoadSegmentReadItem BuildReadItem(RoadSegmentWasAdded e)
    {
        return new RoadSegmentReadItem
        {
            RoadSegmentId = e.RoadSegmentId,
            Geometry = new RoadSegmentGeometryProjections
            {
                Lambert72 = e.Geometry.EnsureLambert72(),
                Lambert08 = e.Geometry.EnsureLambert08()
            },
            StartNodeId = e.StartNodeId,
            EndNodeId = e.EndNodeId,
            GeometryDrawMethod = e.GeometryDrawMethod.ToString(),
            Status = e.Status.ToString(),
            AccessRestriction = ToStringAttribute(e.AccessRestriction),
            Category = ToStringAttribute(e.Category),
            Morphology = ToStringAttribute(e.Morphology),
            StreetNameId = new ReadRoadSegmentDynamicAttribute<RoadSegmentStreetNameAttributeValue>(e.StreetNameId.Values
                .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentStreetNameAttributeValue?)new RoadSegmentStreetNameAttributeValue
                {
                    StreetNameId = x.Value,
                    DutchName = null
                }))),
            MaintenanceAuthorityId = new ReadRoadSegmentDynamicAttribute<RoadSegmentMaintenanceAuthorityAttributeValue>(e.MaintenanceAuthorityId.Values
                .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (RoadSegmentMaintenanceAuthorityAttributeValue?)new RoadSegmentMaintenanceAuthorityAttributeValue
                {
                    OrganizationId = x.Value,
                    Name = null
                }))),
            SurfaceType = ToStringAttribute(e.SurfaceType),
            CarTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.CarTrafficDirection),
            BikeTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentTrafficDirection>(e.BikeTrafficDirection),
            PedestrianTrafficDirection = new ReadRoadSegmentDynamicAttribute<RoadSegmentPedestrianTrafficDirection>(e.PedestrianTrafficDirection),
            EuropeanRoadNumbers = e.EuropeanRoadNumbers.ToList(),
            NationalRoadNumbers = e.NationalRoadNumbers.ToList(),
            Origin = e.Provenance.ToEventTimestamp(),
            LastModified = e.Provenance.ToEventTimestamp(),
            IsV2 = true
        };
    }

    private static ReadRoadSegmentDynamicAttribute<string> ToStringAttribute<T>(RoadSegmentDynamicAttributeValues<T> attribute)
        where T : notnull
    {
        return new ReadRoadSegmentDynamicAttribute<string>(attribute.Values
            .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (string?)x.Value!.ToString())));
    }
}
