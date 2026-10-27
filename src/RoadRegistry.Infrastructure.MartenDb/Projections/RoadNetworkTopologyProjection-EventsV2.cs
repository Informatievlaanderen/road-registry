namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using Marten;
using RoadNode.Events.V2;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadSegment.Events.V2;
using RoadSegment.ValueObjects;
using ScopedRoadNetwork.Events.V2;

public partial class RoadNetworkTopologyProjection
{
    public void Project(IEvent<RoadNodeWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_roadnode(?, ?, ?, ?, TRUE);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID
        );
    }

    public void Project(IEvent<RoadNodeWasModified> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadnode(?, ?, ?, ?, TRUE, null);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry?.WKT ?? string.Empty,
            e.Data.Geometry?.SRID ?? 0
        );
    }

    public void Project(IEvent<RoadNodeWasMigrated> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadnode(?, ?, ?, ?, TRUE, null);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID
        );
    }

    // Nothing is deleted from the topology for a V2 event. The projection only decides which identifiers a road
    // network is loaded by, and a removed entity has to come along: the domain is the one that knows it is removed,
    // and every action has to say so for itself rather than be handed a road network where it silently never existed.
    //
    // That also settles what happens when this projection is eventually dropped and the identifiers are read straight
    // off the road network - removed entities will be there just the same.
    //
    // The row is marked instead, because a caller that only wants what is really there - an extract - has to be able
    // to tell. Removing marks through networktopology_remove_<entity>; the one thing that un-removes anything, the
    // road node removal correction below, rides on the update.
    public void Project(IEvent<RoadNodeWasRemoved> e, IDocumentOperations ops) => ProjectRoadNodeRemoved(e.Data.RoadNodeId, e.Timestamp, ops);
    public void Project(IEvent<RoadNodeWasRemovedBecauseOfMigration> e, IDocumentOperations ops) => ProjectRoadNodeRemoved(e.Data.RoadNodeId, e.Timestamp, ops);

    private static void ProjectRoadNodeRemoved(RoadNodeId roadNodeId, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_remove_roadnode(?, ?);",
            roadNodeId.ToInt32(),
            timestamp
        );
    }

    // The removal is undone, so the mark goes with it. The row itself was never deleted and keeps its geometry; the
    // RoadNodeWasMigrated that follows this, when the road node is collected again, updates it as usual.
    public void Project(IEvent<RoadNodeRemovalWasCorrectedBecauseOfMigration> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadnode(?, ?, '', 0, TRUE, FALSE);",
            e.Data.RoadNodeId.ToInt32(),
            e.Timestamp
        );
    }

    public void Project(IEvent<RoadSegmentWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_roadsegment(?, ?, ?, ?, ?, ?, TRUE, ?);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            (object?)e.Data.StartNodeId?.ToInt32() ?? DBNull.Value,
            (object?)e.Data.EndNodeId?.ToInt32() ?? DBNull.Value,
            e.Data.Status.ToString()
        );
    }

    public void Project(IEvent<OutlinedRoadSegmentWasAdded> e, IDocumentOperations ops)
    {
        // An outlined segment is not knotted into the network; it has no road nodes.
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_roadsegment(?, ?, ?, ?, null, null, TRUE, ?);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            e.Data.Status.ToString()
        );
    }

    public void Project(IEvent<RoadSegmentWasMerged> e, IDocumentOperations ops)
    {
        // A merge takes a road segment back into the network, removed or not, the same as the aggregate does.
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE, ?, FALSE);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            (object?)e.Data.StartNodeId?.ToInt32() ?? DBNull.Value,
            (object?)e.Data.EndNodeId?.ToInt32() ?? DBNull.Value,
            e.Data.Status.ToString()
        );
    }

    public void Project(IEvent<RoadSegmentGeometryWasModified> e, IDocumentOperations ops)
    {
        // The geometry moves, the status does not.
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE, '', null);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT,
            e.Data.Geometry.SRID,
            (object?)e.Data.StartNodeId?.ToInt32() ?? DBNull.Value,
            (object?)e.Data.EndNodeId?.ToInt32() ?? DBNull.Value
        );
    }

    public void Project(IEvent<RoadSegmentWasModified> e, IDocumentOperations ops)
    {
        if (e.Data.Geometry is null && e.Data.NodeIds is null && e.Data.Status is null)
        {
            return;
        }

        // Absent NodeIds means the nodes did not change (-1); present NodeIds carries the full new node
        // state, where an absent id becomes NULL because the segment has no node there. An absent status likewise
        // means the status did not move.
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE, ?, null);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry?.WKT ?? string.Empty,
            e.Data.Geometry?.SRID ?? 0,
            e.Data.NodeIds is null ? -1 : (object?)e.Data.NodeIds.Start?.ToInt32() ?? DBNull.Value,
            e.Data.NodeIds is null ? -1 : (object?)e.Data.NodeIds.End?.ToInt32() ?? DBNull.Value,
            e.Data.Status?.ToString() ?? string.Empty
        );
    }

    // The status changes. Each transition raises an event of its own, but what the topology has to do with one
    // follows entirely from the kind of change it is: a segment is hooked onto road nodes, comes loose from them, or -
    // outside the network before and after - keeps the road nodes it has, which is none. All three record the status
    // itself; RoadSegmentStatusChange.ForEvent supplies it.

    public void Project(IEvent<RoadSegmentWasRealizedFromPlanned> e, IDocumentOperations ops) => ProjectConnected(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasRealizedFromOutOfUse> e, IDocumentOperations ops) => ProjectConnected(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasCorrectedFromHistorizedToRealized> e, IDocumentOperations ops) => ProjectConnected(e.Data, e.Timestamp, ops);

    public void Project(IEvent<RoadSegmentWasCorrectedFromRealizedToPlanned> e, IDocumentOperations ops) => ProjectDisconnected(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasTakenOutOfUseFromRealized> e, IDocumentOperations ops) => ProjectDisconnected(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasHistorizedFromRealized> e, IDocumentOperations ops) => ProjectDisconnected(e.Data, e.Timestamp, ops);

    public void Project(IEvent<RoadSegmentWasHistorizedFromOutOfUse> e, IDocumentOperations ops) => ProjectUnconnectedStatusChange(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasCorrectedFromNotRealizedToPlanned> e, IDocumentOperations ops) => ProjectUnconnectedStatusChange(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasCorrectedFromHistorizedToOutOfUse> e, IDocumentOperations ops) => ProjectUnconnectedStatusChange(e.Data, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasNotRealizedFromPlanned> e, IDocumentOperations ops) => ProjectUnconnectedStatusChange(e.Data, e.Timestamp, ops);

    private static void ProjectConnected(IRoadSegmentWasConnectedEvent m, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE, ?, null);",
            m.RoadSegmentId.ToInt32(),
            timestamp,
            m.Geometry.WKT,
            m.Geometry.SRID,
            m.StartNodeId.ToInt32(),
            m.EndNodeId.ToInt32(),
            RoadSegmentStatusChange.ForEvent(m).To.ToString()
        );
    }

    private static void ProjectDisconnected(IRoadSegmentWasDisconnectedEvent m, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ProjectDisconnected(m.RoadSegmentId, RoadSegmentStatusChange.ForEvent(m).To, timestamp, ops);
    }

    private static void ProjectDisconnected(RoadSegmentId roadSegmentId, RoadSegmentStatusV2 status, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        // The segment came loose from its road nodes: NULL clears them, the empty wkt keeps the geometry.
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, '', 0, null, null, TRUE, ?, null);",
            roadSegmentId.ToInt32(),
            timestamp,
            status.ToString()
        );
    }

    // The segment was outside the network before and stays outside it: it carries no road nodes either way, so the
    // status is the only thing that moves.
    private static void ProjectUnconnectedStatusChange(IRoadSegmentUnconnectedStatusChangeEvent m, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, '', 0, -1, -1, TRUE, ?, null);",
            m.RoadSegmentId.ToInt32(),
            timestamp,
            RoadSegmentStatusChange.ForEvent(m).To.ToString()
        );
    }

    public void Project(IEvent<RoadSegmentWasMigrated> e, IDocumentOperations ops)
    {
        // A road segment that was removed and is collected again arrives here, so this un-removes it, the same as
        // the aggregate does.
        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE, ?, FALSE);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Geometry.WKT ,
            e.Data.Geometry.SRID,
            (object?)e.Data.StartNodeId?.ToInt32() ?? DBNull.Value,
            (object?)e.Data.EndNodeId?.ToInt32() ?? DBNull.Value,
            e.Data.Status.ToString()
        );
    }

    // The row stays - see the note above RoadNodeWasRemoved - but it is marked removed, so a caller that only
    // wants the road segments that are really there can leave it out.
    public void Project(IEvent<RoadSegmentWasRemoved> e, IDocumentOperations ops) => ProjectRemoved(e.Data.RoadSegmentId, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasRemovedBecauseOfMigration> e, IDocumentOperations ops) => ProjectRemoved(e.Data.RoadSegmentId, e.Timestamp, ops);

    private static void ProjectRemoved(RoadSegmentId roadSegmentId, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_remove_roadsegment(?, ?);",
            roadSegmentId.ToInt32(),
            timestamp
        );
    }

    // A segment retired by a merger or a split is historized, not removed: the aggregate keeps it - status gehistoreerd,
    // no road nodes - so the topology keeps it as well, the same as a segment historized from realized.
    public void Project(IEvent<RoadSegmentWasRetiredBecauseOfMerger> e, IDocumentOperations ops) => ProjectDisconnected(e.Data.RoadSegmentId, RoadSegmentStatusV2.Gehistoreerd, e.Timestamp, ops);
    public void Project(IEvent<RoadSegmentWasRetiredBecauseOfSplit> e, IDocumentOperations ops) => ProjectDisconnected(e.Data.RoadSegmentId, RoadSegmentStatusV2.Gehistoreerd, e.Timestamp, ops);

    public void Project(IEvent<RoadSegmentWasSplit> e, IDocumentOperations ops)
    {
        // Only the segment that keeps its identifier (largest part) carries modifications; update its geometry and
        // road nodes. A split does not move its status.
        if (e.Data.Modifications is null)
        {
            return;
        }

        ops.QueueSqlCommand("SELECT projections.networktopology_update_roadsegment(?, ?, ?, ?, ?, ?, TRUE, '', null);",
            e.Data.RoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.Modifications.Geometry.WKT,
            e.Data.Modifications.Geometry.SRID,
            (object?)e.Data.Modifications.StartNodeId?.ToInt32() ?? DBNull.Value,
            (object?)e.Data.Modifications.EndNodeId?.ToInt32() ?? DBNull.Value
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_gradeseparatedjunction(?, ?, ?, ?, TRUE);",
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.LowerRoadSegmentId.ToInt32(),
            e.Data.UpperRoadSegmentId.ToInt32()
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

    public void Project(IEvent<GradeSeparatedJunctionWasMigrated> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand($"UPDATE {GradeSeparatedJunctionsTableName} SET lower_road_segment_id = ?, upper_road_segment_id = ?, timestamp = ?, is_v2 = TRUE WHERE id = ?",
            e.Data.LowerRoadSegmentId.ToInt32(),
            e.Data.UpperRoadSegmentId.ToInt32(),
            e.Timestamp,
            e.Data.GradeSeparatedJunctionId.ToInt32()
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasRemoved> e, IDocumentOperations ops) => ProjectGradeSeparatedJunctionRemoved(e.Data.GradeSeparatedJunctionId, e.Timestamp, ops);
    public void Project(IEvent<GradeSeparatedJunctionWasRemovedBecauseOfMigration> e, IDocumentOperations ops) => ProjectGradeSeparatedJunctionRemoved(e.Data.GradeSeparatedJunctionId, e.Timestamp, ops);

    private static void ProjectGradeSeparatedJunctionRemoved(GradeSeparatedJunctionId gradeSeparatedJunctionId, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_remove_gradeseparatedjunction(?, ?);",
            gradeSeparatedJunctionId.ToInt32(),
            timestamp
        );
    }

    public void Project(IEvent<GradeJunctionWasAdded> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_gradejunction(?, ?, ?, ?, TRUE);",
            e.Data.GradeJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.RoadSegmentId1.ToInt32(),
            e.Data.RoadSegmentId2.ToInt32()
        );
    }

    public void Project(IEvent<GradeJunctionWasModified> e, IDocumentOperations ops)
    {
        if (e.Data.RoadSegmentId1 is null && e.Data.RoadSegmentId2 is null)
        {
            return;
        }

        ops.QueueSqlCommand("SELECT projections.networktopology_update_gradejunction(?, ?, ?, ?);",
            e.Data.GradeJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.RoadSegmentId1?.ToInt32() ?? 0,
            e.Data.RoadSegmentId2?.ToInt32() ?? 0
        );
    }

    public void Project(IEvent<GradeJunctionWasRemoved> e, IDocumentOperations ops) => ProjectGradeJunctionRemoved(e.Data.GradeJunctionId, e.Timestamp, ops);

    // The crossing did not disappear, it is recorded as a grade separated junction from here on. The grade junction
    // it was is marked removed on the aggregate, and the topology keeps the row and marks it the same way.
    public void Project(IEvent<GradeJunctionWasChangedToGradeSeparatedJunction> e, IDocumentOperations ops) => ProjectGradeJunctionRemoved(e.Data.GradeJunctionId, e.Timestamp, ops);

    private static void ProjectGradeJunctionRemoved(GradeJunctionId gradeJunctionId, DateTimeOffset timestamp, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_remove_gradejunction(?, ?);",
            gradeJunctionId.ToInt32(),
            timestamp
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_gradeseparatedjunction(?, ?, ?, ?, TRUE);",
            e.Data.GradeSeparatedJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.LowerRoadSegmentId.ToInt32(),
            e.Data.UpperRoadSegmentId.ToInt32()
        );
    }

    public void Project(IEvent<GradeSeparatedJunctionWasChangedToGradeJunction> e, IDocumentOperations ops)
    {
    }

    public void Project(IEvent<GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange> e, IDocumentOperations ops)
    {
        ops.QueueSqlCommand("SELECT projections.networktopology_insert_gradejunction(?, ?, ?, ?, TRUE);",
            e.Data.GradeJunctionId.ToInt32(),
            e.Timestamp,
            e.Data.RoadSegmentId1.ToInt32(),
            e.Data.RoadSegmentId2.ToInt32()
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
    public void Project(IEvent<StreetNameWasCreated> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<StreetNameWasModified> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentStreetNameIdWasChanged> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<RoadSegmentGeometryDrawMethodWasChanged> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<StreetNameWasRemoved> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<StreetNameWasRenamed> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<OrganizationWasImported> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<OrganizationWasCreated> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<OrganizationWasModified> e, IDocumentOperations ops)
    {
        // Do nothing
    }
    public void Project(IEvent<OrganizationWasRemoved> e, IDocumentOperations ops)
    {
        // Do nothing
    }
}
