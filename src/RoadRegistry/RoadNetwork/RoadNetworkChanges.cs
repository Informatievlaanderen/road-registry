namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BackOffice;
using GradeSeparatedJunction.Changes;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;

public class RoadNetworkChanges : IReadOnlyCollection<IRoadNetworkChange>
{
    public int Count => _changes.Count;

    public IReadOnlyCollection<RoadNodeId> RoadNodeIds { get; }
    public IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; }
    public IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds { get; }

    private readonly List<Geometry> _geometries = [];
    private readonly List<IRoadNetworkChange> _changes = [];
    private readonly List<RoadNodeId> _roadNodeIds = [];
    private readonly List<RoadSegmentId> _roadSegmentIds = [];
    private readonly List<GradeSeparatedJunctionId> _gradeSeparatedJunctionIds = [];

    private RoadNetworkChanges()
    {
        RoadNodeIds = _roadNodeIds.AsReadOnly();
        RoadSegmentIds = _roadSegmentIds.AsReadOnly();
        GradeSeparatedJunctionIds = _gradeSeparatedJunctionIds.AsReadOnly();
    }

    public static RoadNetworkChanges Start()
    {
        return new RoadNetworkChanges();
    }

    public IEnumerator<IRoadNetworkChange> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public MultiPolygon BuildScopeGeometry()
    {
        if (!_geometries.Any())
        {
            return MultiPolygon.Empty;
        }

        var scopes = new List<Polygon>();

        foreach (var geometry in _geometries)
        {
            var boundingBox = (Polygon)geometry.Buffer(1).ConvexHull();

            var scope = scopes.FirstOrDefault(x => x.Intersects(boundingBox));
            if (scope is not null)
            {
                scopes.Remove(scope);
                scope = (Polygon)scope.Union(boundingBox);
            }
            else
            {
                scope = boundingBox;
            }

            scopes.Add(scope);
        }

        MergeOverlappingScopes(scopes);

        return new MultiPolygon(scopes.ToArray())
            .WithSrid(_geometries.First().SRID);
    }

    private void MergeOverlappingScopes(List<Polygon> scopes)
    {
        for (var i = 0; i < scopes.Count; i++)
        {
            for (var j = i + 1; j < scopes.Count; j++)
            {
                if (scopes[i].Intersects(scopes[j]))
                {
                    var merged = (Polygon)scopes[i].Union(scopes[j]);
                    scopes[i] = merged;
                    scopes.RemoveAt(j);
                    j--; // Adjust index after removal
                }
            }
        }
    }

    public RoadNetworkChanges Add(AddRoadNodeChange change)
    {
        _geometries.Add(change.Geometry);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(ModifyRoadNodeChange change)
    {
        if (change.Geometry is not null)
        {
            _geometries.Add(change.Geometry);
        }

        _roadNodeIds.Add(change.RoadNodeId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadNodeChange change)
    {
        _roadNodeIds.Add(change.RoadNodeId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(AddRoadSegmentChange change)
    {
        _geometries.Add(change.Geometry);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(ModifyRoadSegmentChange change)
    {
        if (change.Geometry is not null)
        {
            _geometries.Add(change.Geometry);
        }

        _roadSegmentIds.Add(change.RoadSegmentId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadSegmentChange change)
    {
        _roadSegmentIds.Add(change.RoadSegmentId);
        return AddChange(change);
    }

    // public void Add(RemoveRoadSegmentsChange change)
    // {
    //     AddChange(change);
    //// if we still know this segment, include the geometry as we know it now
    //foreach (var roadSegmentId in removeRoadSegments.Ids)
    //{
    //      SegmentIds.Add(roadSegmentId);
    //dit in aparte commando steken waarbij de repo een aparte method krijgt om de gekoppelde nodes op te halen
    //}
    // }
    //
    public RoadNetworkChanges Add(AddRoadSegmentToEuropeanRoadChange change)
    {
        _roadSegmentIds.Add(change.RoadSegmentId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadSegmentFromEuropeanRoadChange change)
    {
        _roadSegmentIds.Add(change.RoadSegmentId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(AddRoadSegmentToNationalRoadChange change)
    {
        _roadSegmentIds.Add(change.RoadSegmentId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadSegmentFromNationalRoadChange change)
    {
        _roadSegmentIds.Add(change.RoadSegmentId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(AddGradeSeparatedJunctionChange change)
    {
        //TODO-pr AddGradeSeparatedJunctionChange te bekijken of dit wel nodig is
        _roadSegmentIds.Add(change.LowerRoadSegmentId);
        _roadSegmentIds.Add(change.UpperRoadSegmentId);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveGradeSeparatedJunctionChange change)
    {
        _gradeSeparatedJunctionIds.Add(change.GradeSeparatedJunctionId);

        return AddChange(change);
    }

    private RoadNetworkChanges AddChange(IRoadNetworkChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        _changes.Add(change);
        return this;
    }
}
