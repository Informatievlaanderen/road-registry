namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenCorrectRoadSegmentFromRealizedToPlannedV2;

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

public class CorrectRoadSegmentFromRealizedToPlannedV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;
    private readonly RoadNetworkTestDataV2 TestData = new();

    public CorrectRoadSegmentFromRealizedToPlannedV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(int id)
    {
        return _controller.CorrectRoadSegmentFromRealizedToPlannedV2(new RoadSegmentIdValidator(), id, Store, CancellationToken.None);
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

    private CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest _capturedSqsRequest;

    private void CaptureSqsRequest()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => _capturedSqsRequest = (CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());
    }

    private void VerifyNothingWasQueued()
    {
        _mediator.Verify(x => x.Send(It.IsAny<CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenAnExistingRoadSegment_ThenAccepted()
    {
        var id = SeedRoadSegment();

        var result = await Act(id);

        result.Should().BeOfType<AcceptedResult>();
        _mediator.Verify(x => x.Send(It.IsAny<CorrectRoadSegmentFromRealizedToPlannedV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenNonExistingRoadSegment_ThenNotFound()
    {
        // VAL-2
        SeedRoadSegment();

        var result = await Act(999);

        result.Should().BeOfType<NotFoundResult>();
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenRemovedRoadSegment_ThenGone()
    {
        // VAL-3
        var id = SeedRoadSegment(isRemoved: true);

        var result = await Act(id);

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
        await Assert.ThrowsAsync<ValidationException>(() => Act(id));
        VerifyNothingWasQueued();
    }

    [Fact]
    public async Task GivenValidRequest_ThenTheRoadSegmentIdIsSentAlong()
    {
        // The action carries nothing else: everything it does follows from the network around the segment.
        var id = SeedRoadSegment();
        CaptureSqsRequest();

        await Act(id);

        _capturedSqsRequest.Should().NotBeNull();
        _capturedSqsRequest.RoadSegmentId.Should().Be(new RoadSegmentId(id));
    }

    [Fact]
    public async Task GivenCallerWithIngemetenScope_ThenMeasuredRoadSegmentsMayBeModified()
    {
        // VAL-9
        var id = SeedRoadSegment();
        GiveCallerScopes(Scopes.DvWrIngemetenWegBeheer);
        CaptureSqsRequest();

        await Act(id);

        _capturedSqsRequest.MayModifyMeasuredRoadSegments.Should().BeTrue();
    }

    [Fact]
    public async Task GivenCallerWithOnlyGeschetsteWegScope_ThenMeasuredRoadSegmentsMayNotBeModified()
    {
        var id = SeedRoadSegment();
        GiveCallerScopes(Scopes.DvWrGeschetsteWegBeheer);
        CaptureSqsRequest();

        await Act(id);

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

        await Act(id);

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
