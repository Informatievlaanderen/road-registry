namespace RoadRegistry.BackOffice.Api.Tests.V2.RoadSegments.WhenGetRoadSegment;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using RoadRegistry;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Extensions;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using Xunit;

public class GetRoadSegmentTests : V2ReadEndpointTestBase
{
    private readonly RoadSegmentsController _controller;

    public GetRoadSegmentTests()
    {
        _controller = new RoadSegmentsController(CreateControllerContext());
        SetHttpContext(_controller);
    }

    [Fact]
    public async Task GivenExistingRoadSegment_ThenOk()
    {
        var roadSegmentWasAdded = Fixture.Create<RoadSegmentWasAdded>();
        Seed(BuildReadItem(roadSegmentWasAdded));

        var result = await _controller.GetRoadSegmentV2((int)roadSegmentWasAdded.RoadSegmentId, ApiOptions, Store);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var detail = okResult.Value.Should().BeOfType<WegsegmentV2Detail>().Subject;
        detail.Identificator.ObjectId.Should().Be(roadSegmentWasAdded.RoadSegmentId.ToString());
        detail.Wegsegmentstatus.Should().Be(roadSegmentWasAdded.Status.ToDutchString());
    }

    [Fact]
    public async Task GivenUnknownRoadSegment_ThenNotFound()
    {
        var result = await _controller.GetRoadSegmentV2(Fixture.Create<int>(), ApiOptions, Store);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GivenRemovedRoadSegment_ThenGone()
    {
        var roadSegmentWasAdded = Fixture.Create<RoadSegmentWasAdded>();
        var readItem = BuildReadItem(roadSegmentWasAdded);
        readItem.IsRemoved = true;
        Seed(readItem);

        var result = await _controller.GetRoadSegmentV2((int)roadSegmentWasAdded.RoadSegmentId, ApiOptions, Store);

        result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
    }

    private static RoadSegmentReadItem BuildReadItem(RoadSegmentWasAdded e)
    {
        return new RoadSegmentReadItem
        {
            RoadSegmentId = e.RoadSegmentId,
            Geometry = new RoadSegmentGeometryProjections
            {
                Lambert72 = BuildGeometry().EnsureLambert72(),
                Lambert08 = BuildGeometry().EnsureLambert08()
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

    private static RoadSegmentGeometry BuildGeometry()
    {
        return new MultiLineString([new LineString([
                new Coordinate(243234.8929999992, 160239.3830000013),
                new Coordinate(243279.0160000026, 160244.1570000015)
            ])])
            .WithSrid(WellknownSrids.Lambert72)
            .ToRoadSegmentGeometry();
    }

    private static ReadRoadSegmentDynamicAttribute<string> ToStringAttribute<T>(RoadSegmentDynamicAttributeValues<T> attribute)
        where T : notnull
    {
        return new ReadRoadSegmentDynamicAttribute<string>(attribute.Values
            .Select(x => (x.Coverage.From, x.Coverage.To, x.Side, (string?)x.Value!.ToString())));
    }
}
