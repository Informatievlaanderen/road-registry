namespace RoadRegistry.BackOffice.FeatureCompare.V3;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommandHandling.Actions.ChangeRoadNetwork;
using RoadNetwork;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;

public class TranslatedChanges : IReadOnlyCollection<IRoadNetworkChange>, IEquatable<TranslatedChanges>
{
    public static readonly TranslatedChanges Empty = new(
        ImmutableList<IRoadNetworkChange>.Empty,
        ImmutableDictionary<RoadSegmentId, AddRoadSegmentChange>.Empty,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegmentChange>.Empty,
        ImmutableDictionary<RoadSegmentId, RemoveRoadSegmentChange>.Empty);

    private readonly ImmutableList<IRoadNetworkChange> _changes;
    private readonly ImmutableDictionary<RoadSegmentId, AddRoadSegmentChange> _addRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, ModifyRoadSegmentChange> _modifyRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, RemoveRoadSegmentChange> _removeRoadSegmentChanges;

    private TranslatedChanges(
        ImmutableList<IRoadNetworkChange> changes,
        ImmutableDictionary<RoadSegmentId, AddRoadSegmentChange> addRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegmentChange> modifyRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, RemoveRoadSegmentChange> removeRoadSegmentChanges)
    {
        _changes = changes ?? throw new ArgumentNullException(nameof(changes));
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
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadNodeChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadNodeChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges.SetItem(change.TemporaryId, change), _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegmentChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(change.RoadSegmentId, change), _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges.SetItem(change.RoadSegmentId, change));
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToEuropeanRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromEuropeanRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNationalRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNationalRoadChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(AddGradeSeparatedJunctionChange change)
    {
        return new TranslatedChanges(_changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges AppendChange(RemoveGradeSeparatedJunctionChange change)
    {
        return new TranslatedChanges(
            _changes.Add(change),
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges ReplaceChange(AddRoadSegmentChange before, AddRoadSegmentChange after)
    {
        return new TranslatedChanges(
            _changes.SetItem(_changes.IndexOf(before), after),
            _addRoadSegmentChanges.SetItem(before.TemporaryId, after), _modifyRoadSegmentChanges, _removeRoadSegmentChanges);
    }

    public TranslatedChanges ReplaceChange(ModifyRoadSegmentChange before, ModifyRoadSegmentChange after)
    {
        //NOTE: ReplaceChange automatically converts a provisional change into a change - by design.
        return new TranslatedChanges(
                _changes.SetItem(_changes.IndexOf(before), after),
                _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(before.RoadSegmentId, after), _removeRoadSegmentChanges);
    }

    public bool TryFindRoadSegmentChange(RoadSegmentId id, out IRoadNetworkChange change)
    {
        change = _addRoadSegmentChanges.GetValueOrDefault(id)
                 ?? (IRoadNetworkChange?)_modifyRoadSegmentChanges.GetValueOrDefault(id)
                 ?? _removeRoadSegmentChanges.GetValueOrDefault(id);
        return change != null;
    }

    public ChangeRoadNetworkCommand ToChangeRoadNetworkCommand(DownloadId downloadId, TicketId ticketId)
    {
        return new ChangeRoadNetworkCommand
        {
            DownloadId = downloadId,
            Changes = _changes.Select(ToChangeRoadNetworkCommandItem).ToList(),
            TicketId = ticketId
        };
    }

    private static ChangeRoadNetworkCommandItem ToChangeRoadNetworkCommandItem(IRoadNetworkChange change)
    {
        switch (change)
        {
            case AddRoadNodeChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    AddRoadNode = command
                };
            case ModifyRoadNodeChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    ModifyRoadNode = command
                };
            case RemoveRoadNodeChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    RemoveRoadNode = command
                };
            case AddRoadSegmentChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    AddRoadSegment = command
                };
            case ModifyRoadSegmentChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    ModifyRoadSegment = command
                };
            case RemoveRoadSegmentChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    RemoveRoadSegment = command
                };
            case AddRoadSegmentToEuropeanRoadChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    AddRoadSegmentToEuropeanRoad = command
                };
            case RemoveRoadSegmentFromEuropeanRoadChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    RemoveRoadSegmentFromEuropeanRoad = command
                };
            case AddRoadSegmentToNationalRoadChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    AddRoadSegmentToNationalRoad = command
                };
            case RemoveRoadSegmentFromNationalRoadChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    RemoveRoadSegmentFromNationalRoad = command
                };
            case AddGradeSeparatedJunctionChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    AddGradeSeparatedJunction = command
                };
            case RemoveGradeSeparatedJunctionChange command:
                return new ChangeRoadNetworkCommandItem
                {
                    RemoveGradeSeparatedJunction = command
                };
            default:
                throw new NotImplementedException($"No handler for change '{change.GetType().Name}'");
        }
    }

    public bool Equals(TranslatedChanges other)
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

    public override bool Equals(object obj)
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
        return (_changes != null ? _changes.GetHashCode() : 0);
    }
}
