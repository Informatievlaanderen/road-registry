namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Valid;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ValueObjects;

public class RoadNetworkChanges : IReadOnlyCollection<IRoadNetworkChange>
{
    public int Count => _changes.Count;

    public Provenance Provenance { get; private set; }
    public RoadNetworkIds Ids => new(_roadNodeIds.AsReadOnly(),  _roadSegmentIds.AsReadOnly(), _gradeSeparatedJunctionIds.AsReadOnly());

    private readonly List<Geometry> _geometries = [];
    private readonly List<IRoadNetworkChange> _changes = [];
    private readonly List<RoadNodeId> _roadNodeIds = [];
    private readonly List<RoadSegmentId> _roadSegmentIds = [];
    private readonly List<RoadSegmentId> _temporaryRoadSegmentIds = [];
    private readonly List<GradeSeparatedJunctionId> _gradeSeparatedJunctionIds = [];

    private RoadNetworkChanges()
    {
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
        return _changes
            .OrderBy(x => Array.IndexOf(ChangeOrderTypes, x.GetType(), 0, ChangeOrderTypes.Length))
            .GetEnumerator();
    }

    internal static readonly Type[] ChangeOrderTypes = [
        //important before segments
        typeof(AddRoadNodeChange),
        typeof(ModifyRoadNodeChange),
        typeof(MigrateRoadNodeChange),
        typeof(RemoveRoadNodeChange),

        //all the rest
        typeof(AddRoadSegmentChange),
        typeof(AddRoadSegmentToEuropeanRoadChange),
        typeof(AddRoadSegmentToNationalRoadChange),
        typeof(MergeRoadSegmentChange),
        typeof(MigrateRoadSegmentChange),
        typeof(ModifyRoadSegmentChange),
        typeof(RemoveRoadSegmentChange),
        typeof(RemoveRoadSegmentFromEuropeanRoadChange),
        typeof(RemoveRoadSegmentFromNationalRoadChange),
        typeof(AddGradeSeparatedJunctionChange),
        typeof(ModifyGradeSeparatedJunctionChange),
        typeof(RemoveGradeSeparatedJunctionChange),
    ];

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public MultiPolygon? BuildScopeGeometry()
    {
        if (!_geometries.Any())
        {
            return null;
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

        var mp = MergeOverlappingScopes(scopes);
        return mp;
    }

    private MultiPolygon MergeOverlappingScopes(List<Polygon> scopes)
    {
        var factory = scopes[0].Factory;
        var geom = factory.BuildGeometry(scopes);
        var unioned = UnaryUnionOp.Union(geom);

        // Keep separate islands as polygons
        var mergedPolys = new List<Polygon>();
        for (var i = 0; i < unioned.NumGeometries; i++)
            if (unioned.GetGeometryN(i) is Polygon p)
                mergedPolys.Add(p);

        return new MultiPolygon(mergedPolys.ToArray(), _geometries.First().Factory)
            .WithSrid(_geometries.First().SRID);
    }

    public RoadNetworkChanges Add(AddRoadNodeChange change)
    {
        _geometries.Add(change.Geometry.Value);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(ModifyRoadNodeChange change)
    {
        if (change.Geometry is not null)
        {
            _geometries.Add(change.Geometry.Value);
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
        _geometries.Add(change.Geometry.Value);

        return AddChange(change);
    }

    public RoadNetworkChanges Add(ModifyRoadSegmentChange change)
    {
        if (change.Geometry is not null)
        {
            _geometries.Add(change.Geometry.Value);
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
