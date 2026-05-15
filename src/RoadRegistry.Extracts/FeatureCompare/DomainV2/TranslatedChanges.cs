namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork;

public sealed class TranslatedChanges : IReadOnlyCollection<IRoadNetworkChange>, IEquatable<TranslatedChanges>
{
    public static TranslatedChanges Empty => new();

    private readonly List<IRoadNetworkChange> _changes = new();
    private readonly Dictionary<RoadNodeId, AddRoadNodeChange> _addRoadNodeChanges = new();
    private readonly Dictionary<RoadNodeId, ModifyRoadNodeChange> _modifyRoadNodeChanges = new();
    private readonly Dictionary<RoadSegmentId, IndexedAddRoadSegmentChange> _addRoadSegmentChanges = new();
    private readonly Dictionary<RoadSegmentId, ModifyRoadSegmentChange> _modifyRoadSegmentChanges = new();
    private readonly Dictionary<RoadSegmentId, RemoveRoadSegmentChange> _removeRoadSegmentChanges = new();
    private readonly List<RoadSegmentId> _identicalRoadSegmentIds = new();

    private readonly record struct IndexedAddRoadSegmentChange(int Index, AddRoadSegmentChange Change);

    private TranslatedChanges()
    {
    }

    public int Count => _changes.Count;

    public IEnumerator<IRoadNetworkChange> GetEnumerator() => _changes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IReadOnlyCollection<RoadSegmentId> GetIdenticalRoadSegmentIds() => _identicalRoadSegmentIds;

    public TranslatedChanges AppendChange(AddRoadNodeChange change)
    {
        _changes.Add(change);
        _addRoadNodeChanges[change.TemporaryId] = change;
        return this;
    }

    public TranslatedChanges AppendChange(ModifyRoadNodeChange change)
    {
        _changes.Add(change);
        _modifyRoadNodeChanges[change.RoadNodeId] = change;
        return this;
    }

    public TranslatedChanges AppendChange(RemoveRoadNodeChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(AddRoadSegmentChange change)
    {
        var roadSegmentId = change.RoadSegmentIdReference.RoadSegmentId;
        _addRoadSegmentChanges[roadSegmentId] = new IndexedAddRoadSegmentChange(_changes.Count, change);
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(ModifyRoadSegmentChange change)
    {
        _changes.Add(change);
        _modifyRoadSegmentChanges[change.RoadSegmentIdReference.RoadSegmentId] = change;
        return this;
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentChange change)
    {
        _changes.Add(change);
        _removeRoadSegmentChanges[change.RoadSegmentId] = change;
        return this;
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToEuropeanRoadChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromEuropeanRoadChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNationalRoadChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNationalRoadChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(AddGradeSeparatedJunctionChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendChange(RemoveGradeSeparatedJunctionChange change)
    {
        _changes.Add(change);
        return this;
    }

    public TranslatedChanges AppendIdenticalRoadSegmentId(RoadSegmentId roadSegmentId)
    {
        _identicalRoadSegmentIds.Add(roadSegmentId);
        return this;
    }

    public TranslatedChanges ReplaceChange(ModifyRoadNodeChange before, RemoveRoadNodeChange after)
    {
        // Not on the hot path: called once per consumed road node in ProcessSchijnknopen.
        var index = _changes.IndexOf(before);
        _changes[index] = after;
        return this;
    }

    public TranslatedChanges ReplaceChange(AddRoadSegmentChange before, AddRoadSegmentChange after)
    {
        // Hot path: called once per uploaded road segment from the European/National road translators.
        // We rely on the stored index so this is O(1) instead of O(N) via _changes.IndexOf.
        var roadSegmentId = before.RoadSegmentIdReference.RoadSegmentId;
        var indexed = _addRoadSegmentChanges[roadSegmentId];
        _changes[indexed.Index] = after;
        _addRoadSegmentChanges[roadSegmentId] = indexed with { Change = after };
        return this;
    }

    public TranslatedChanges ReplaceChange(ModifyRoadSegmentChange before, ModifyRoadSegmentChange after)
    {
        // NOTE: ReplaceChange automatically converts a provisional change into a change - by design.
        // Not on the hot path: ModifyRoadSegmentChange has no replace caller in production today, only tests.
        var index = _changes.IndexOf(before);
        _changes[index] = after;
        _modifyRoadSegmentChanges[before.RoadSegmentIdReference.RoadSegmentId] = after;
        return this;
    }

    public bool TryFindModifyRoadNodeChange(RoadNodeId id, [NotNullWhen(true)] out ModifyRoadNodeChange? change)
    {
        change = _modifyRoadNodeChanges.GetValueOrDefault(id);
        return change is not null;
    }

    public TranslatedChanges RemoveAddedRoadNodeChange(RoadNodeId id)
    {
        if (!_addRoadNodeChanges.TryGetValue(id, out var change))
        {
            return this;
        }

        // _changes.IndexOf + RemoveAt is O(N), but this is called only for unused temporary schijnknopen
        // — a tiny fraction of all changes — so it stays out of the hot path.
        var removedIndex = _changes.IndexOf(change);
        _changes.RemoveAt(removedIndex);
        _addRoadNodeChanges.Remove(id);
        ShiftAddRoadSegmentIndexesAfter(removedIndex);
        return this;
    }

    /// <summary>
    /// Keeps the tracked positions in <c>_addRoadSegmentChanges</c> aligned with <c>_changes</c> after a
    /// remove-at shifted everything past <paramref name="removedIndex"/> down by one.
    /// </summary>
    private void ShiftAddRoadSegmentIndexesAfter(int removedIndex)
    {
        // ToList() to avoid mutating the dictionary while enumerating its keys.
        var keys = _addRoadSegmentChanges.Keys.ToList();
        foreach (var key in keys)
        {
            var indexed = _addRoadSegmentChanges[key];
            if (indexed.Index > removedIndex)
            {
                _addRoadSegmentChanges[key] = indexed with { Index = indexed.Index - 1 };
            }
        }
    }

    public bool TryFindRoadSegmentChange(RoadSegmentId id, out IRoadNetworkChange? change)
    {
        if (_addRoadSegmentChanges.TryGetValue(id, out var addIndexed))
        {
            change = addIndexed.Change;
            return true;
        }

        if (_modifyRoadSegmentChanges.TryGetValue(id, out var modify))
        {
            change = modify;
            return true;
        }

        if (_removeRoadSegmentChanges.TryGetValue(id, out var remove))
        {
            change = remove;
            return true;
        }

        change = null;
        return false;
    }

    public bool Equals(TranslatedChanges? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _changes.SequenceEqual(other._changes);
        // exclude _identicalRoadSegmentIds since it's only informational for Inwinning
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((TranslatedChanges)obj);
    }

    public override int GetHashCode()
    {
        // Reference-based via the default object hash code. Mutable instances cannot have a stable structural hash,
        // and nothing in the codebase uses TranslatedChanges as a dictionary key.
        return base.GetHashCode();
    }
}
