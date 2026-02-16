namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using Marten;
using RoadNode.Events.V2;
using RoadSegment.Events.V2;
using ScopedRoadNetwork.Events.V2;

public partial class RoadNetworkTopologyProjection
{
    public void Project(IEvent<RoadNodeWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {RoadNodesTableName} (id, geometry, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, TRUE)",
            e.Data.RoadNodeId.ToInt32(),
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadNodeWasModified> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadnode(?, ?, ?, ?, TRUE);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry?.WKT ?? string.Empty,
            e.Data.Geometry?.SRID ?? 0
        );
    }

    public void Project(IEvent<RoadNodeWasMigrated> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadnode(?, ?, ?, ?, TRUE);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID
        );
    }

    public void Project(IEvent<RoadNodeWasRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadnode(?, ?);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?, TRUE)",
            e.Data.RoadSegmentId.ToInt32(),
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            e.Data.StartNodeId.ToInt32(),
            e.Data.EndNodeId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentWasMerged> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp, is_v2) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?, TRUE)",
            e.Data.RoadSegmentId.ToInt32(),
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            e.Data.StartNodeId.ToInt32(),
            e.Data.EndNodeId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentWasModified> e, IDocumentOperations ops)
    {
        if (e.Data.Geometry is null && e.Data.StartNodeId is null && e.Data.EndNodeId is null)
        {
            return;
        }

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry?.WKT ?? string.Empty,
            e.Data.Geometry?.SRID ?? 0,
            e.Data.StartNodeId?.ToInt32() ?? 0,
            e.Data.EndNodeId?.ToInt32() ?? 0
        );
    }

    public void Project(IEvent<RoadSegmentWasMigrated> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT ,
            e.Data.Geometry.SRID,
            e.Data.StartNodeId.ToInt32(),
            e.Data.EndNodeId.ToInt32()
        );
    }

    public void Project(IEvent<RoadSegmentWasRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadsegment(?, ?);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentWasRetiredBecauseOfMerger> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadsegment(?, ?);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentWasRetiredBecauseOfMigration> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadsegment(?, ?);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, timestamp, is_v2) VALUES (?, ?, ?, ?, TRUE)",
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Data.LowerRoadSegmentId.ToInt32(),
            e.Data.UpperRoadSegmentId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasModified> e, IDocumentOperations ops)
    {
        if (e.Data.LowerRoadSegmentId is null && e.Data.UpperRoadSegmentId is null)
        {
            return;
        }

        ops.QueueSqlCommand("SELECT projections.networktopology_update_gradeseparatedjunction(?, ?, ?, ?);",
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.LowerRoadSegmentId?.ToInt32() ?? 0,
            e.Data.UpperRoadSegmentId?.ToInt32() ?? 0
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasRemoved> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_delete_gradeseparatedjunction(?, ?);",
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadNetworkWasChanged> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadNodeTypeWasChanged> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentWasAddedToEuropeanRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentWasAddedToNationalRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentWasRemovedFromEuropeanRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentWasRemovedFromNationalRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }
}
