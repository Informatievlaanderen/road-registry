namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Quadtree;

public class LazyQuadtree<T> : ISpatialIndex<T>
{
    private readonly Action<Quadtree<T>> _build;
    private bool _invalidated;
    private Quadtree<T>? _tree;

    public LazyQuadtree(Action<Quadtree<T>> build)
    {
        _build = build;
    }

    public void Rebuild()
    {
        _tree = new Quadtree<T>();
        _build(_tree);
        _invalidated = false;
    }

    public void Invalidate()
    {
        _invalidated = true;
    }

    public void Insert(Envelope itemEnv, T item)
    {
        if (_tree is not null)
        {
            _tree.Insert(itemEnv, item);
        }
    }

    public bool Remove(Envelope itemEnv, T item)
    {
        if (_tree is not null)
        {
            return _tree.Remove(itemEnv, item);
        }

        return true;
    }

    public void Update(Envelope oldEnvelope, Envelope newEnvelope, T item)
    {
        if (_tree is not null && !oldEnvelope.Equals(newEnvelope))
        {
            _tree.Remove(oldEnvelope, item);
            _tree.Insert(newEnvelope, item);
        }
    }

    public void Query(Envelope searchEnv, IItemVisitor<T> visitor)
    {
        EnsureTreeIsBuilt();

        _tree!.Query(searchEnv, visitor);
    }
    public IList<T> Query(Envelope searchEnv)
    {
        EnsureTreeIsBuilt();

        return _tree!.Query(searchEnv);
    }

    private void EnsureTreeIsBuilt()
    {
        if (_tree is null || _invalidated)
        {
            Rebuild();
        }
    }
}
