namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ChangeRoadSegmentStatus;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// Shared scaffolding for the three shapes a status change can have. Each shape has its own test class; what they have
// in common is how a small road network is put together.
public abstract class StatusChangeAggregateTestBase : AggregateTestBase
{
    // The segment whose status is being changed.
    protected const int ChangedSegmentId = 1;
    protected const int NeighbourSegmentId = 2;
    protected const int SecondNeighbourSegmentId = 3;

    protected RoadNodeWasAdded BuildNode(int id, double x, double y, RoadNodeTypeV2? type = null)
    {
        return new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
            Grensknoop = false,
            Type = type,
            Provenance = new ProvenanceData(TestData.Provenance)
        };
    }

    protected RoadSegmentGeometry BuildGeometry(params (double X, double Y)[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates.Select(x => new Coordinate(x.X, x.Y)).ToArray())])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }

    private static RoadSegmentDynamicAttributeValues<T> Spanning<T>(RoadSegmentDynamicAttributeValues<T> template, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>().Add(template.Values.First().Value, geometry);
    }

    // Every attribute value covers the whole segment, so the segment is internally consistent with its own geometry.
    protected RoadSegmentWasAdded BuildSegment(
        int id,
        RoadNodeWasAdded? startNode,
        RoadNodeWasAdded? endNode,
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2 status,
        RoadSegmentGeometryDrawMethodV2? geometryDrawMethod = null)
    {
        var template = TestData.Segment1Added;

        return template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode?.RoadNodeId,
            EndNodeId = endNode?.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = geometryDrawMethod ?? RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = status,
            AccessRestriction = Spanning(template.AccessRestriction, geometry),
            Category = Spanning(template.Category, geometry),
            Morphology = Spanning(template.Morphology, geometry),
            StreetNameId = Spanning(template.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(template.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(template.SurfaceType, geometry),
            CarTrafficDirection = Spanning(template.CarTrafficDirection, geometry),
            BikeTrafficDirection = Spanning(template.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = Spanning(template.PedestrianTrafficDirection, geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
    }

    protected ScopedRoadNetwork BuildNetwork(
        RoadNodeWasAdded[] nodes,
        RoadSegmentWasAdded[] segments,
        GradeSeparatedJunctionWasAdded[]? gradeSeparatedJunctions = null,
        GradeJunctionWasAdded[]? gradeJunctions = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray(),
            (gradeSeparatedJunctions ?? []).Select(x => GradeSeparatedJunction.Create(x).WithoutChanges()).ToArray(),
            (gradeJunctions ?? []).Select(x => GradeJunction.Create(x).WithoutChanges()).ToArray()).WithoutChanges();
    }

    protected static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    protected RoadNetworkChangeResult Act(
        ScopedRoadNetwork roadNetwork,
        RoadSegmentStatusChange statusChange,
        bool mayModifyMeasuredRoadSegments = true,
        int roadSegmentId = ChangedSegmentId)
    {
        return roadNetwork.ChangeRoadSegmentStatus(
            statusChange,
            new RoadSegmentId(roadSegmentId),
            mayModifyMeasuredRoadSegments,
            IdGenerator(),
            TestData.Provenance);
    }
}
