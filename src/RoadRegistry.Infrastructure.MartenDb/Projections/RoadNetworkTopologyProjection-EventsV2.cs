namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using Marten;
using RoadNetwork.Events.V2;
using RoadNode.Events.V2;
using RoadSegment.Events.V2;

public partial class RoadNetworkTopologyProjection
{
    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?)",
            e.Data.RoadSegmentId.ToInt32(), e.Data.Geometry.WKT, e.Data.Geometry.SRID, e.Data.StartNodeId.ToInt32(), e.Data.EndNodeId.ToInt32(), e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        if (e.Data.Geometry is null && e.Data.StartNodeId is null && e.Data.EndNodeId is null)
        {
            return;
        }

        var parameters = new object?[]
        {
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry?.WKT,
            e.Data.Geometry?.SRID,
            e.Data.StartNodeId?.ToInt32(),
            e.Data.EndNodeId?.ToInt32()
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?);", parameters);
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        var parameters = new object[]
        {
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadsegment(?, ?);", parameters);
    }

    public void Project(IEvent<GradeSeparatedJunctionAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, timestamp) VALUES (?, ?, ?, ?)",
            e.Data.GradeSeparatedJunctionId.ToInt32(), e.Data.LowerRoadSegmentId.ToInt32(), e.Data.UpperRoadSegmentId.ToInt32(), e.Timestamp
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionModified> e, IDocumentOperations ops)
    {
        if (e.Data.LowerRoadSegmentId is null && e.Data.UpperRoadSegmentId is null)
        {
            return;
        }

        var parameters = new object?[]
        {
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.LowerRoadSegmentId?.ToInt32(),
            e.Data.UpperRoadSegmentId?.ToInt32()
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_update_gradeseparatedjunction(?, ?, ?, ?);", parameters);
    }

    public void Project(IEvent<GradeSeparatedJunctionRemoved> e, IDocumentOperations ops)
    {
        var parameters = new object[]
        {
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Timestamp
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_delete_gradeseparatedjunction(?, ?);", parameters);
    }

    public void Project(IEvent<RoadNodeAdded> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadNodeModified> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadNodeRemoved> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadNetworkChanged> e, IDocumentOperations ops)
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
    public void Project(IEvent<RoadSegmentRemovedFromEuropeanRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentRemovedFromNationalRoad> e, IDocumentOperations ops)
    {
        // Do nothing
    }
}
