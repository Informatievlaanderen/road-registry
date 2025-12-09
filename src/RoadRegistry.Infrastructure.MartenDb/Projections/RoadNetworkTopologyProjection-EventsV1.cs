namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using GradeSeparatedJunction.Events.V1;
using JasperFx.Events;
using Marten;
using RoadNetwork.Events.V1;
using RoadNode.Events.V1;
using RoadSegment;
using RoadSegment.Events.V1;

public partial class RoadNetworkTopologyProjection
{
    public void Project(IEvent<ImportedRoadSegment> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?)",
            e.Data.Id, e.Data.Geometry.WKT, e.Data.Geometry.SpatialReferenceSystemIdentifier, e.Data.StartNodeId, e.Data.EndNodeId, e.Timestamp
        );
    }
    public void Project(IEvent<RoadSegmentAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {RoadSegmentsTableName} (id, geometry, start_node_id, end_node_id, timestamp) VALUES (?, ST_GeomFromText(?, ?), ?, ?, ?)",
            e.Data.Id, e.Data.Geometry.WKT, e.Data.Geometry.SpatialReferenceSystemIdentifier, e.Data.StartNodeId, e.Data.EndNodeId, e.Timestamp
        );
    }
    public void Project(IEvent<RoadSegmentModified> e, IDocumentOperations ops)
    {
        var parameters = new object?[]
        {
            e.Data.Id,
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SpatialReferenceSystemIdentifier,
            e.Data.StartNodeId,
            e.Data.EndNodeId
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?);", parameters);
    }
    public void Project(IEvent<RoadSegmentGeometryModified> e, IDocumentOperations ops)
    {
        var parameters = new object?[]
        {
            e.Data.Id,
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SpatialReferenceSystemIdentifier
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, null, null);", parameters);
    }

    public void Project(IEvent<RoadSegmentRemoved> e, IDocumentOperations ops)
    {
        var parameters = new object[]
        {
            e.Data.Id,
            e.Timestamp
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_delete_roadsegment(?, ?);", parameters);
    }

    public void Project(IEvent<ImportedGradeSeparatedJunction> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, timestamp) VALUES (?, ?, ?, ?)",
            e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Timestamp
        );
    }
    public void Project(IEvent<GradeSeparatedJunctionAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"INSERT INTO {GradeSeparatedJunctionsTableName} (id, lower_road_segment_id, upper_road_segment_id, timestamp) VALUES (?, ?, ?, ?)",
            e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Timestamp
        );
    }
    public void Project(IEvent<GradeSeparatedJunctionRemoved> e, IDocumentOperations ops)
    {
        var parameters = new object[]
        {
            e.Data.Id,
            e.Timestamp
        };

        ops.QueueSqlCommand("SELECT projections.networktopology_delete_gradeseparatedjunction(?, ?);", parameters);
    }

    public void Project(IEvent<ImportedRoadNode> e, IDocumentOperations ops)
    {
        // Do nothing
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
