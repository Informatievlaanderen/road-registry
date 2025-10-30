namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BackOffice;
using Changes;
using NetTopologySuite.Geometries;
using RoadSegment.ValueObjects;

public class RoadNetworkChanges : IReadOnlyCollection<IRoadNetworkChange>
{
    public int Count => _changes.Count;

    public List<RoadNodeId> RoadNodeIds { get; } = [];
    public List<RoadSegmentId> RoadSegmentIds { get; } = [];
    public List<GradeSeparatedJunctionId> GradeSeparatedJunctionIds { get; } = []; //TODO-pr fill

    private readonly List<Geometry> _geometries = [];
    private readonly List<IRoadNetworkChange> _changes = [];

    private RoadNetworkChanges()
    {
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

    //TODO-pr implement other change types
    public RoadNetworkChanges Add(AddRoadNodeChange change)
    {
        _geometries.Add(change.Geometry);

        return AddChange(change);
    }

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

        RoadSegmentIds.Add(change.Id);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadSegmentChange change)
    {
        RoadSegmentIds.Add(change.Id);
        return AddChange(change);
    }

    // public void Add(RemoveRoadSegmentsChange change)
    // {
    //     AddChange(change);
    //// if we still know this segment, include the geometry as we know it now
    //foreach (var roadSegmentId in removeRoadSegments.Ids)
    //{
    //      SegmentIds.Add(roadSegmentId);
    //TODO-pr hoe gekoppelde nodes ook toevoegen? dit laten gebeuren in RoadNetworkRepo? -> dit in aparte commando steken waarbij de repo een aparte method krijgt om de gekoppelde nodes op te halen
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
    public void Add(AddGradeSeparatedJunctionChange change)
    {
        AddChange(change);
    }
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

    private RoadNetworkChanges AddChange(IRoadNetworkChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        _changes.Add(change);
        return this;
    }

    public static RoadNetworkChanges Start()
    {
        return new RoadNetworkChanges();
    }
}
