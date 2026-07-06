namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenSplitRoadSegmentV2;

using System.Linq;
using AutoFixture;
using BackOffice.Abstractions.Exceptions;
using BackOffice.Handlers.Sqs.RoadSegments.V2;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;

public class SplitRoadSegmentV2Tests : V2ReadEndpointTestBase
{
    // Lambert 2008 point on the seeded segment (0,0)->(50,50)->(100,100)
    private const string ValidCutPositionLambert08 = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>50 50</gml:pos></gml:Point>";
    private const string CutPositionLambert72 = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/31370"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>50 50</gml:pos></gml:Point>";
    private const string CutPositionTooFar = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>50 60</gml:pos></gml:Point>";
    // On the seeded segment (line y = x), but with sub-centimeter precision (more than 2 decimals)
    private const string CutPositionSubCmPrecision = @"<gml:Point srsName=""https://www.opengis.net/def/crs/EPSG/0/3812"" xmlns:gml=""http://www.opengis.net/gml/3.2""><gml:pos>50.123456789 50.123456789</gml:pos></gml:Point>";

    private readonly Mock<IMediator> _mediator = new();
    private readonly RoadSegmentsController _controller;
    private readonly RoadNetworkTestDataV2 TestData = new();

    public SplitRoadSegmentV2Tests()
    {
        _mediator
            .Setup(x => x.Send(It.IsAny<SplitRoadSegmentV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Fixture.Create<LocationResult>());

        _controller = new RoadSegmentsController(CreateControllerContext(), _mediator.Object);
        SetHttpContext(_controller);
    }

    private Task<IActionResult> Act(int id, string knippositie)
    {
        return _controller.SplitRoadSegmentV2(
            new RoadSegmentIdValidator(),
            Store,
            new SplitRoadSegmentV2Parameters { Knippositie = knippositie },
            id,
            CancellationToken.None);
    }

    private int SeedRealizedRoadSegment(string statusOverride = null, bool isV2 = true)
    {
        var e = TestData.Segment1Added;
        Seed(BuildReadItem(e, statusOverride, isV2));
        return (int)e.RoadSegmentId;
    }

    [Fact]
    public async Task GivenValidRequest_ThenAccepted()
    {
        var id = SeedRealizedRoadSegment();

        var result = await Act(id, ValidCutPositionLambert08);

        result.Should().BeOfType<AcceptedResult>();
        _mediator.Verify(x => x.Send(It.IsAny<SplitRoadSegmentV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenKnippositieWithSubCentimeterPrecision_ThenAccepted()
    {
        var id = SeedRealizedRoadSegment();

        var result = await Act(id, CutPositionSubCmPrecision);

        result.Should().BeOfType<AcceptedResult>();
        _mediator.Verify(x => x.Send(It.IsAny<SplitRoadSegmentV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenValidRequest_ThenCutPositionSentAsPoint()
    {
        var id = SeedRealizedRoadSegment();
        SplitRoadSegmentV2SqsRequest captured = null;
        _mediator
            .Setup(x => x.Send(It.IsAny<SplitRoadSegmentV2SqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((r, _) => captured = (SplitRoadSegmentV2SqsRequest)r)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        await Act(id, ValidCutPositionLambert08);

        captured.Should().NotBeNull();
        captured.CutPosition.Should().BeOfType<Point>();
        captured.CutPosition.SRID.Should().Be(WellknownSrids.Lambert08);
        captured.CutPosition.X.Should().Be(50);
        captured.CutPosition.Y.Should().Be(50);
    }

    [Fact]
    public async Task GivenNonExistingRoadSegment_ThenRoadSegmentNotFoundException()
    {
        await Assert.ThrowsAsync<RoadSegmentNotFoundException>(() => Act(999, ValidCutPositionLambert08));
        _mediator.Verify(x => x.Send(It.IsAny<SplitRoadSegmentV2SqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenRemovedRoadSegment_ThenRoadSegmentNotFoundException()
    {
        var e = TestData.Segment1Added;
        var readItem = BuildReadItem(e, null, true);
        readItem.IsRemoved = true;
        Seed(readItem);

        await Assert.ThrowsAsync<RoadSegmentNotFoundException>(() => Act((int)e.RoadSegmentId, ValidCutPositionLambert08));
    }

    [Fact]
    public async Task GivenNotV2RoadSegment_ThenValidationException()
    {
        var id = SeedRealizedRoadSegment(isV2: false);

        await Assert.ThrowsAsync<ValidationException>(() => Act(id, ValidCutPositionLambert08));
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    public async Task GivenInvalidStatus_ThenValidationException(string status)
    {
        var id = SeedRealizedRoadSegment(status);

        await Assert.ThrowsAsync<ValidationException>(() => Act(id, ValidCutPositionLambert08));
    }

    [Fact]
    public async Task GivenKnippositieNull_ThenValidationException()
    {
        var id = SeedRealizedRoadSegment();

        await Assert.ThrowsAsync<ValidationException>(() => Act(id, null));
    }

    [Fact]
    public async Task GivenKnippositieNotLambert08_ThenValidationException()
    {
        var id = SeedRealizedRoadSegment();

        await Assert.ThrowsAsync<ValidationException>(() => Act(id, CutPositionLambert72));
    }

    [Fact]
    public async Task GivenKnippositieTooFarFromRoadSegment_ThenValidationException()
    {
        var id = SeedRealizedRoadSegment();

        await Assert.ThrowsAsync<ValidationException>(() => Act(id, CutPositionTooFar));
    }

    private static RoadSegmentReadItem BuildReadItem(RoadSegmentWasAdded e, string statusOverride, bool isV2)
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
            IsV2 = isV2
        };
    }

    private static ReadRoadSegmentDynamicAttribute<string> ToStringAttribute<T>(RoadSegmentDynamicAttributeValues<T> attribute)
        where T : notnull
    {
        return new ReadRoadSegmentDynamicAttribute<string>(attribute.Values
            .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (string?)x.Value!.ToString())));
    }
}
