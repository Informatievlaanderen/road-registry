namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenSplitRoadSegmentsByJunctionV2;

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
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;

public class SplitRoadSegmentsByJunctionV2Tests : V2ReadEndpointTestBase
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;
    private readonly RoadNetworkTestDataV2 TestData = new();

    public SplitRoadSegmentsByJunctionV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<SplitRoadSegmentsByJunctionV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(int? wegsegment1, int? wegsegment2)
    {
        return _controller.SplitRoadSegmentsByJunctionV2(
            Store,
            new SplitRoadSegmentsByJunctionV2Parameters { Wegsegment1 = wegsegment1, Wegsegment2 = wegsegment2 },
            CancellationToken.None);
    }

    private int SeedRealizedRoadSegment(RoadSegmentWasAdded e, string statusOverride = null)
    {
        Seed(BuildReadItem(e, statusOverride));
        return (int)e.RoadSegmentId;
    }

    private RoadSegmentWasAdded Segment2 => TestData.Segment1Added with { RoadSegmentId = new RoadSegmentId(TestData.Segment1Added.RoadSegmentId.ToInt32() + 1) };

    [Fact]
    public async Task GivenValidRequest_ThenAcceptedAndBothIdsSent()
    {
        var id1 = SeedRealizedRoadSegment(TestData.Segment1Added);
        var id2 = SeedRealizedRoadSegment(Segment2);

        SplitRoadSegmentsByJunctionV2SqsRequest captured = null;
        _mediator
            .Setup(x => x.Send(It.IsAny<SplitRoadSegmentsByJunctionV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => captured = (SplitRoadSegmentsByJunctionV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        var result = await Act(id1, id2);

        result.Should().BeOfType<AcceptedResult>();
        captured.Should().NotBeNull();
        captured!.RoadSegmentId1.ToInt32().Should().Be(id1);
        captured.RoadSegmentId2.ToInt32().Should().Be(id2);
    }

    [Fact]
    public async Task GivenWegsegment1Null_ThenValidationException()
    {
        var id2 = SeedRealizedRoadSegment(Segment2);

        await Assert.ThrowsAsync<ValidationException>(() => Act(null, id2));
        _mediator.Verify(x => x.Send(It.IsAny<SplitRoadSegmentsByJunctionV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenNonExistingRoadSegment_ThenValidationException()
    {
        var id1 = SeedRealizedRoadSegment(TestData.Segment1Added);

        await Assert.ThrowsAsync<ValidationException>(() => Act(id1, 999999));
        _mediator.Verify(x => x.Send(It.IsAny<SplitRoadSegmentsByJunctionV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenRemovedRoadSegment_ThenValidationException()
    {
        var id1 = SeedRealizedRoadSegment(TestData.Segment1Added);
        var readItem = BuildReadItem(Segment2, null);
        readItem.IsRemoved = true;
        Seed(readItem);

        await Assert.ThrowsAsync<ValidationException>(() => Act(id1, (int)Segment2.RoadSegmentId));
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public async Task GivenNotRealizedStatus_ThenValidationException(string status)
    {
        var id1 = SeedRealizedRoadSegment(TestData.Segment1Added);
        var id2 = SeedRealizedRoadSegment(Segment2, status);

        await Assert.ThrowsAsync<ValidationException>(() => Act(id1, id2));
    }

    private static RoadSegmentReadItem BuildReadItem(RoadSegmentWasAdded e, string statusOverride)
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
            Status = statusOverride ?? e.Status.ToString(),
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
