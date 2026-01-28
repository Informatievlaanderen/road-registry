namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using GradeSeparatedJunction.Events.V1;
using JasperFx.Events;
using Marten;
using RoadNode.Events.V1;
using RoadSegment.Events.V1;
using ScopedRoadNetwork.Events.V1;

public partial class RoadNetworkTopologyProjection
{
    public void Project(IEvent<ImportedRoadNode> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand($"INSERT INTO {RoadNodesTableName} (id, geometry, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, FALSE)",
            e.Data.RoadNodeId,
            geometry.WKT,
            geometry.SRID,
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadNodeAdded> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand($"INSERT INTO {RoadNodesTableName} (id, geometry, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, FALSE)",
            e.Data.RoadNodeId,
            geometry.WKT,
            geometry.SRID,
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadNodeModified> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadnode(?, ?, ?, ?, FALSE);",
            e.Data.RoadNodeId,
            e.Timestamp,
            geometry.WKT,
            geometry.SRID
        );
    }

    public void Project(IEvent<RoadNodeRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadnode(?, ?);",
            e.Data.RoadNodeId,
            e.Timestamp
        );
    }

    public void Project(IEvent<ImportedRoadSegment> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?, FALSE)",
            e.Data.RoadSegmentId,
            geometry.WKT,
            geometry.SRID,
            e.Data.StartNodeId,
            e.Data.EndNodeId,
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?, FALSE)",
            e.Data.RoadSegmentId,
            geometry.WKT,
            geometry.SRID,
            e.Data.StartNodeId,
            e.Data.EndNodeId,
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, FALSE);",
            e.Data.RoadSegmentId,
            e.Timestamp,
            geometry.WKT,
            geometry.SRID,
            e.Data.StartNodeId,
            e.Data.EndNodeId
        );
    }

    public void Project(IEvent<RoadSegmentGeometryModified> e, IDocumentOperations ops)
    {
        var geometry = e.Data.Geometry.ToLambert08();

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, null, null, FALSE);",
            e.Data.RoadSegmentId,
            e.Timestamp,
            geometry.WKT,
            geometry.SRID
        );
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadsegment(?, ?);",
            e.Data.RoadSegmentId,
            e.Timestamp
        );
    }

    public void Project(IEvent<ImportedGradeSeparatedJunction> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, timestamp, is_v2) VALUES (?, ?, ?, ?, FALSE)",
            e.Data.Id,
            e.Data.LowerRoadSegmentId,
            e.Data.UpperRoadSegmentId,
            e.Timestamp
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, timestamp, is_v2) VALUES (?, ?, ?, ?, FALSE)",
            e.Data.Id,
            e.Data.LowerRoadSegmentId,
            e.Data.UpperRoadSegmentId,
            e.Timestamp
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionModified> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_gradeseparatedjunction(?, ?, ?, ?);",
            e.Data.Id,
            e.Timestamp,
            e.Data.LowerRoadSegmentId,
            e.Data.UpperRoadSegmentId
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_gradeseparatedjunction(?, ?);",
            e.Data.Id,
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadNetworkChangesAccepted> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentAttributesModified> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentAddedToEuropeanRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentAddedToNationalRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentAddedToNumberedRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentRemovedFromEuropeanRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentRemovedFromNationalRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentRemovedFromNumberedRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<OutlinedRoadSegmentRemoved> e, IDocumentOperations ops)
    {
        // Do nothing
    }

    public void Project(IEvent<RoadSegmentStreetNamesChanged> e, IDocumentOperations ops)
    {
        // Do nothing
    }
}
