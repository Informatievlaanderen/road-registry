namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;

public class TranslatedChanges : IReadOnlyCollection<ITranslatedChange>
{
    public static readonly TranslatedChanges Empty = new(
        Reason.None,
        OperatorName.None,
        OrganizationId.Unknown,
        ImmutableList<ITranslatedChange>.Empty,
        ImmutableList<ITranslatedChange>.Empty);

    private readonly ImmutableList<ITranslatedChange> _changes;
    private readonly ImmutableList<ITranslatedChange> _provisionalChanges;

    private TranslatedChanges(Reason reason,
        OperatorName @operator,
        OrganizationId organization,
        ImmutableList<ITranslatedChange> changes,
        ImmutableList<ITranslatedChange> provisionalChanges)
    {
        Reason = reason;
        Operator = @operator;
        Organization = organization;
        _changes = changes ?? throw new ArgumentNullException(nameof(changes));
        _provisionalChanges = provisionalChanges ?? throw new ArgumentNullException(nameof(provisionalChanges));
    }

    public int Count => _changes.Count;
    public OperatorName Operator { get; }
    public OrganizationId Organization { get; }
    public Reason Reason { get; }

    public IEnumerator<ITranslatedChange> GetEnumerator()
    {
        return _changes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TranslatedChanges AppendChange(AddRoadNode change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadNode change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadNode change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegmentAttributes change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegmentGeometry change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToEuropeanRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromEuropeanRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNationalRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNationalRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNumberedRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegmentOnNumberedRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNumberedRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(AddGradeSeparatedJunction change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyGradeSeparatedJunction change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveGradeSeparatedJunction change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _provisionalChanges);
    }

    public TranslatedChanges AppendProvisionalChange(ModifyRoadSegment change)
    {
        //NOTE: Only road segment modifications are currently considered provisional
        return new TranslatedChanges(Reason, Operator, Organization, _changes, _provisionalChanges.Add(change));
    }

    public TranslatedChanges ReplaceChange(AddRoadNode before, AddRoadNode after)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges);
    }

    public TranslatedChanges ReplaceChange(ModifyRoadNode before, ModifyRoadNode after)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges);
    }

    public TranslatedChanges ReplaceChange(AddRoadSegment before, AddRoadSegment after)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges);
    }

    public TranslatedChanges ReplaceChange(ModifyRoadSegment before, ModifyRoadSegment after)
    {
        //NOTE: ReplaceChange automatically converts a provisional change into a change - by design.
        return _provisionalChanges.Contains(before)
            ? new TranslatedChanges(Reason, Operator, Organization, _changes.Add(after), _provisionalChanges.Remove(before))
            : new TranslatedChanges(Reason, Operator, Organization, _changes.Remove(before).Add(after), _provisionalChanges);
    }

    public TranslatedChanges ReplaceProvisionalChange(ModifyRoadSegment before, ModifyRoadSegment after)
    {
        //NOTE: ReplaceProvisionalChange replaces an existing provisional change (if found).
        return _provisionalChanges.Contains(before)
            ? new TranslatedChanges(Reason, Operator, Organization, _changes, _provisionalChanges.Remove(before).Add(after))
            : this;
    }

    public bool TryFindRoadNodeChangeOfShapeRecord(RecordNumber number, out ITranslatedChange change)
    {
        change = new ITranslatedChange[]
        {
            _changes.OfType<AddRoadNode>().SingleOrDefault(_ => _.RecordNumber.Equals(number)),
            _changes.OfType<ModifyRoadNode>().SingleOrDefault(_ => _.RecordNumber.Equals(number)),
            _changes.OfType<RemoveRoadNode>().SingleOrDefault(_ => _.RecordNumber.Equals(number))
        }.Flatten();
        return change != null;
    }

    public bool TryFindRoadSegmentChange(RoadSegmentId id, out ITranslatedChange change)
    {
        change = new ITranslatedChange[]
        {
            _changes.OfType<AddRoadSegment>().SingleOrDefault(_ => _.TemporaryId == id),
            _changes.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.Id == id),
            _changes.OfType<RemoveRoadSegment>().SingleOrDefault(_ => _.Id == id)
        }.Flatten();
        return change != null;
    }

    public bool TryFindRoadSegmentChange(RecordNumber number, out ITranslatedChange change)
    {
        change = new ITranslatedChange[]
        {
            _changes.OfType<AddRoadSegment>().SingleOrDefault(_ => _.RecordNumber.Equals(number)),
            _changes.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.RecordNumber.Equals(number)),
            _changes.OfType<RemoveRoadSegment>().SingleOrDefault(_ => _.RecordNumber.Equals(number))
        }.Flatten();
        return change != null;
    }

    // For Dynamic Attribute Records
    public bool TryFindRoadSegmentProvisionalChange(RoadSegmentId id, out ITranslatedChange change)
    {
        //NOTE: Only road segment modifications are currently considered provisional
        change = new ITranslatedChange[]
        {
            _provisionalChanges.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.Id == id)
        }.Flatten();
        return change != null;
    }

    // For Shape Records
    public bool TryFindRoadSegmentProvisionalChange(RecordNumber number, out ITranslatedChange change)
    {
        //NOTE: Only road segment modifications are currently considered provisional
        change = new ITranslatedChange[]
        {
            _provisionalChanges.OfType<ModifyRoadSegment>().SingleOrDefault(_ => _.RecordNumber.Equals(number))
        }.Flatten();
        return change != null;
    }

    public TranslatedChanges WithOperatorName(OperatorName value)
    {
        return new TranslatedChanges(Reason, value, Organization, _changes, _provisionalChanges);
    }

    public TranslatedChanges WithOrganization(OrganizationId value)
    {
        return new TranslatedChanges(Reason, Operator, value, _changes, _provisionalChanges);
    }

    public TranslatedChanges WithReason(Reason value)
    {
        return new TranslatedChanges(value, Operator, Organization, _changes, _provisionalChanges);
    }
}
