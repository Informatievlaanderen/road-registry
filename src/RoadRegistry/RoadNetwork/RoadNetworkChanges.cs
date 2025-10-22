namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections;
using System.Collections.Generic;
using BackOffice;
using Changes;
using NetTopologySuite.Geometries;
using RoadSegment.ValueObjects;

public class RoadNetworkChanges : IReadOnlyCollection<IRoadNetworkChange>
{
    public int Count => _changes.Count;

    public TransactionId TransactionId { get; }
    public List<RoadNodeId> NodeIds { get; }
    public List<RoadSegmentId> SegmentIds { get; }
    public Envelope Scope { get; }

    private readonly List<IRoadNetworkChange> _changes;

    private RoadNetworkChanges(TransactionId transactionId)
    {
        TransactionId = transactionId;
        Scope = new();
    }

    public IEnumerator<IRoadNetworkChange> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    //TODO-pr implement other change types
    // public void Add(AddRoadNodeChange change)
    // {
    //     AppendChange(change);
    //envelope.ExpandToInclude(addRoadNode.Geometry.Coordinate);
    // }

    // public void Add(ModifyRoadNodeChange change)
    // {
    //     AppendChange(change);

    // NodeIds.Add(change.Id);

    //     if (modifyRoadNode.Geometry is not null)
    //     {
    //         // the geometry to modify it to
    //         envelope.ExpandToInclude(modifyRoadNode.Geometry.Coordinate);
    //     }
    // }
    //
    // public void Add(RemoveRoadNodeChange change)
    // {
    //     AppendChange(change);
    // NodeIds.Add(change.Id);
    // }

    public void Add(AddRoadSegmentChange change)
    {
        AddChange(change);

        Scope.ExpandToInclude(change.Geometry.EnvelopeInternal);
    }

    public void Add(ModifyRoadSegmentChange change)
    {
        AddChange(change);

        if (change.Geometry is not null)
        {
            Scope.ExpandToInclude(change.Geometry.EnvelopeInternal);
        }

        SegmentIds.Add(change.Id);
    }

    // public void Add(RemoveRoadSegmentChange change)
    // {
    //     AddChange(change);
    //     SegmentIds.Add(change.Id);
    // }
    //
    // public void Add(RemoveRoadSegmentsChange change)
    // {
    //     AddChange(change);
    //// if we still know this segment, include the geometry as we know it now
    //foreach (var roadSegmentId in removeRoadSegments.Ids)
    //{
    //      SegmentIds.Add(roadSegmentId);
    //TODO-pr hoe gekoppelde nodes ook toevoegen? dit laten gebeuren in RoadNetworkRepo?
    //}
    // }
    //
    // public void Add(RemoveOutlinedRoadSegmentChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(RemoveOutlinedRoadSegmentFromRoadNetworkChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(AddRoadSegmentToEuropeanRoadChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(RemoveRoadSegmentFromEuropeanRoadChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(AddRoadSegmentToNationalRoadChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(RemoveRoadSegmentFromNationalRoadChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(AddRoadSegmentToNumberedRoadChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(RemoveRoadSegmentFromNumberedRoadChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(AddGradeSeparatedJunctionChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(ModifyGradeSeparatedJunctionChange change)
    // {
    //     AddChange(change);
    // }
    //
    // public void Add(RemoveGradeSeparatedJunctionChange change)
    // {
    //     AddChange(change);
    // }

    private void AddChange(IRoadNetworkChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        _changes.Add(change);
    }

    public static RoadNetworkChanges Start(TransactionId transactionId)
    {
        return new RoadNetworkChanges(transactionId);
    }
}
