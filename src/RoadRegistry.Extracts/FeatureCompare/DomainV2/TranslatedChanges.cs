namespace RoadRegistry.Extracts.FeatureCompare.DomainV2;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ScopedRoadNetwork;

public sealed class TranslatedChanges : IReadOnlyCollection<IRoadNetworkChange>, IEquatable<TranslatedChanges>
{
    public static readonly TranslatedChanges Empty = new(
        ImmutableList<IRoadNetworkChange>.Empty,
        ImmutableDictionary<RoadNodeId, ModifyRoadNodeChange>.Empty,
        ImmutableDictionary<RoadSegmentId, AddRoadSegmentChange>.Empty,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegmentChange>.Empty,
        ImmutableDictionary<RoadSegmentId, RemoveRoadSegmentChange>.Empty);

    private readonly ImmutableList<IRoadNetworkChange> _changes;
    private readonly ImmutableDictionary<RoadNodeId, ModifyRoadNodeChange> _modifyRoadNodeChanges;
    private readonly ImmutableDictionary<RoadSegmentId, AddRoadSegmentChange> _addRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, ModifyRoadSegmentChange> _modifyRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, RemoveRoadSegmentChange> _removeRoadSegmentChanges;

    private TranslatedChanges(
        ImmutableList<IRoadNetworkChange> changes,
        ImmutableDictionary<RoadNodeId, ModifyRoadNodeChange> modifyRoadNodeChanges,
        ImmutableDictionary<RoadSegmentId, AddRoadSegmentChange> addRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegmentChange> modifyRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, RemoveRoadSegmentChange> removeRoadSegmentChanges)
    {
        _changes = changes ?? throw new ArgumentNullException(nameof(changes));
        _modifyRoadNodeChanges = modifyRoadNodeChanges.ThrowIfNull();
        _addRoadSegmentChanges = addRoadSegmentChanges.ThrowIfNull();
        _modifyRoadSegmentChanges = modifyRoadSegmentChanges.ThrowIfNull();
        _removeRoadSegmentChanges = removeRoadSegmentChanges.ThrowIfNull();
    }

    public int Count => _changes.Count;

    public IEnumerator<IRoadNetworkChange> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TranslatedChanges AppendChange(AddRoadNodeChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadNodeChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges.SetItem(change.RoadNodeId, change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadNodeChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges.SetItem(change.RoadSegmentIdReference.RoadSegmentId, change), _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegmentChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(change.RoadSegmentIdReference.RoadSegmentId, change), _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges.SetItem(change.RoadSegmentId, change));
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToEuropeanRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromEuropeanRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNationalRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNationalRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(AddGradeSeparatedJunctionChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _modifyRoadNodeChanges, _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveGradeSeparatedJunctionChange change)
    {
        return new TranslatedChanges(
            _changes.Add(change),
            _modifyRoadNodeChanges,
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges ReplaceChange(ModifyRoadNodeChange before, RemoveRoadNodeChange after)
    {
        return new TranslatedChanges(
            _changes.SetItem(_changes.IndexOf(before), after),
            _modifyRoadNodeChanges,
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges ReplaceChange(AddRoadSegmentChange before, AddRoadSegmentChange after)
    {
        return new TranslatedChanges(
            _changes.SetItem(_changes.IndexOf(before), after),
            _modifyRoadNodeChanges,
            _addRoadSegmentChanges.SetItem(before.RoadSegmentIdReference.RoadSegmentId, after), _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges ReplaceChange(ModifyRoadSegmentChange before, ModifyRoadSegmentChange after)
    {
        //NOTE: ReplaceChange automatically converts a provisional change into a change - by design.
        return new TranslatedChanges(
                _changes.SetItem(_changes.IndexOf(before), after),
                _modifyRoadNodeChanges,
                _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(before.RoadSegmentIdReference.RoadSegmentId, after), _removeRoadSegmentChanges);
    }

    public bool TryFindModifyRoadNodeChange(RoadNodeId id, [NotNullWhen(true)] out ModifyRoadNodeChange? change)
    {
        change = _modifyRoadNodeChanges.GetValueOrDefault(id);
        return change != null;
    }

    public bool TryFindRoadSegmentChange(RoadSegmentId id, out IRoadNetworkChange? change)
    {
        change = _addRoadSegmentChanges.GetValueOrDefault(id)
                 ?? (IRoadNetworkChange?)_modifyRoadSegmentChanges.GetValueOrDefault(id)
                 ?? _removeRoadSegmentChanges.GetValueOrDefault(id);
        return change != null;
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
        return _changes.GetHashCode();
    }
}
