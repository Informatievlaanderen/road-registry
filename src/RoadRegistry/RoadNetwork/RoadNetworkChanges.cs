namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Extensions;
using GradeSeparatedJunction.Changes;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.ValueObjects;
using RoadSegment.Changes;

public class RoadNetworkChanges : IReadOnlyCollection<IRoadNetworkChange>
{
    public int Count => _changes.Count;

    public Provenance Provenance { get; private set; }
    public IReadOnlyCollection<RoadNodeId> RoadNodeIds { get; }
    public IReadOnlyCollection<RoadSegmentId> RoadSegmentIds { get; }
    public IReadOnlyCollection<GradeSeparatedJunctionId> GradeSeparatedJunctionIds { get; }

    private readonly List<Geometry> _geometries = [];
    private readonly List<IRoadNetworkChange> _changes = [];
    private readonly List<RoadNodeId> _roadNodeIds = [];
    private readonly List<RoadSegmentId> _roadSegmentIds = [];
    private readonly List<RoadSegmentId> _temporaryRoadSegmentIds = [];
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

    public RoadNetworkChanges WithProvenance(Provenance provenance)
    {
        Provenance = provenance;
        return this;
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
        _temporaryRoadSegmentIds.Add(change.TemporaryId);
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

    public RoadNetworkChanges Add(AddRoadSegmentToEuropeanRoadChange change)
    {
        if (!_temporaryRoadSegmentIds.Contains(change.RoadSegmentId))
        {
            _roadSegmentIds.Add(change.RoadSegmentId);
        }

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadSegmentFromEuropeanRoadChange change)
    {
        if (!_temporaryRoadSegmentIds.Contains(change.RoadSegmentId))
        {
            _roadSegmentIds.Add(change.RoadSegmentId);
        }

        return AddChange(change);
    }

    public RoadNetworkChanges Add(AddRoadSegmentToNationalRoadChange change)
    {
        if (!_temporaryRoadSegmentIds.Contains(change.RoadSegmentId))
        {
            _roadSegmentIds.Add(change.RoadSegmentId);
        }

        return AddChange(change);
    }

    public RoadNetworkChanges Add(RemoveRoadSegmentFromNationalRoadChange change)
    {
        if (!_temporaryRoadSegmentIds.Contains(change.RoadSegmentId))
        {
            _roadSegmentIds.Add(change.RoadSegmentId);
        }

        return AddChange(change);
    }

    public RoadNetworkChanges Add(AddGradeSeparatedJunctionChange change)
    {
        if (!_temporaryRoadSegmentIds.Contains(change.LowerRoadSegmentId))
        {
            _roadSegmentIds.Add(change.LowerRoadSegmentId);
        }

        if (!_temporaryRoadSegmentIds.Contains(change.UpperRoadSegmentId))
        {
            _roadSegmentIds.Add(change.UpperRoadSegmentId);
        }

        return AddChange(change);
    }

    public RoadNetworkChanges Add(ModifyGradeSeparatedJunctionChange change)
    {
        _gradeSeparatedJunctionIds.Add(change.GradeSeparatedJunctionId);

        if (change.LowerRoadSegmentId is not null && !_temporaryRoadSegmentIds.Contains(change.LowerRoadSegmentId.Value))
        {
            _roadSegmentIds.Add(change.LowerRoadSegmentId.Value);
        }

        if (change.UpperRoadSegmentId is not null && !_temporaryRoadSegmentIds.Contains(change.UpperRoadSegmentId.Value))
        {
            _roadSegmentIds.Add(change.UpperRoadSegmentId.Value);
        }

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
