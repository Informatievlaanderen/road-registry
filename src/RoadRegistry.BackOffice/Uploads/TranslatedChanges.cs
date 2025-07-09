namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Exceptions;
using Messages;
using Microsoft.Extensions.Logging;

public class TranslatedChanges : IReadOnlyCollection<ITranslatedChange>
{
    public static readonly TranslatedChanges Empty = new(
        Reason.None,
        OperatorName.Unknown,
        OrganizationId.Unknown,
        ImmutableList<ITranslatedChange>.Empty,
        ImmutableDictionary<RoadSegmentId, AddRoadSegment>.Empty,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegment>.Empty,
        ImmutableDictionary<RoadSegmentId, RemoveRoadSegment>.Empty,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegment>.Empty);

    private readonly ImmutableList<ITranslatedChange> _changes;
    private readonly ImmutableDictionary<RoadSegmentId, AddRoadSegment> _addRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, ModifyRoadSegment> _modifyRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, RemoveRoadSegment> _removeRoadSegmentChanges;
    private readonly ImmutableDictionary<RoadSegmentId, ModifyRoadSegment> _modifyRoadSegmentProvisionalChanges;

    private TranslatedChanges(Reason reason,
        OperatorName @operator,
        OrganizationId organization,
        ImmutableList<ITranslatedChange> changes,
        ImmutableDictionary<RoadSegmentId, AddRoadSegment> addRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegment> modifyRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, RemoveRoadSegment> removeRoadSegmentChanges,
        ImmutableDictionary<RoadSegmentId, ModifyRoadSegment> modifyRoadSegmentProvisionalChanges)
    {
        Reason = reason;
        Operator = @operator;
        Organization = organization;
        _changes = changes ?? throw new ArgumentNullException(nameof(changes));
        _addRoadSegmentChanges = addRoadSegmentChanges.ThrowIfNull();
        _modifyRoadSegmentChanges = modifyRoadSegmentChanges.ThrowIfNull();
        _removeRoadSegmentChanges = removeRoadSegmentChanges.ThrowIfNull();
        _modifyRoadSegmentProvisionalChanges = modifyRoadSegmentProvisionalChanges.ThrowIfNull();
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
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadNode change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadNode change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges.SetItem(change.TemporaryId, change), _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(change.Id, change), _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges.SetItem(change.Id, change), _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveOutlinedRoadSegment change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveOutlinedRoadSegmentFromRoadNetwork change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToEuropeanRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromEuropeanRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNationalRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNationalRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(AddRoadSegmentToNumberedRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveRoadSegmentFromNumberedRoad change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(AddGradeSeparatedJunction change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(ModifyGradeSeparatedJunction change)
    {
        return new TranslatedChanges(Reason, Operator, Organization, _changes.Add(change), _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges, _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendChange(RemoveGradeSeparatedJunction change)
    {
        return new TranslatedChanges(Reason, Operator, Organization,
            _changes.Add(change),
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
            _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges AppendProvisionalChange(ModifyRoadSegment change)
    {
        //NOTE: Only road segment modifications are currently considered provisional
        return new TranslatedChanges(Reason, Operator, Organization,
            _changes,
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
            _modifyRoadSegmentProvisionalChanges.SetItem(change.Id, change));
    }

    public TranslatedChanges ReplaceChange(AddRoadSegment before, AddRoadSegment after)
    {
        return new TranslatedChanges(Reason, Operator, Organization,
            _changes.SetItem(_changes.IndexOf(before), after),
            _addRoadSegmentChanges.SetItem(before.TemporaryId, after), _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
            _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges ReplaceChange(ModifyRoadSegment before, ModifyRoadSegment after)
    {
        //NOTE: ReplaceChange automatically converts a provisional change into a change - by design.
        return _modifyRoadSegmentProvisionalChanges.ContainsKey(before.Id)
            ? new TranslatedChanges(Reason, Operator, Organization,
                _changes.Add(after),
                _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(after.Id, after), _removeRoadSegmentChanges,
                _modifyRoadSegmentProvisionalChanges.Remove(before.Id))
            : new TranslatedChanges(Reason, Operator, Organization,
                _changes.SetItem(_changes.IndexOf(before), after),
                _addRoadSegmentChanges, _modifyRoadSegmentChanges.SetItem(before.Id, after), _removeRoadSegmentChanges,
                _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges ReplaceProvisionalChange(ModifyRoadSegment before, ModifyRoadSegment after)
    {
        //NOTE: ReplaceProvisionalChange replaces an existing provisional change (if found).
        return _modifyRoadSegmentProvisionalChanges.ContainsKey(before.Id)
            ? new TranslatedChanges(Reason, Operator, Organization,
                _changes,
                _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
                _modifyRoadSegmentProvisionalChanges.SetItem(before.Id, after))
            : this;
    }

    public bool TryFindRoadSegmentChange(RoadSegmentId id, out ITranslatedChange change)
    {
        change = _addRoadSegmentChanges.GetValueOrDefault(id)
                 ?? (ITranslatedChange?)_modifyRoadSegmentChanges.GetValueOrDefault(id)
                 ?? _removeRoadSegmentChanges.GetValueOrDefault(id);
        return change != null;
    }

    // For Dynamic Attribute Records
    public bool TryFindRoadSegmentProvisionalChange(RoadSegmentId id, out ITranslatedChange change)
    {
        //NOTE: Only road segment modifications are currently considered provisional
        change = _modifyRoadSegmentProvisionalChanges.GetValueOrDefault(id);
        return change != null;
    }

    public TranslatedChanges WithOperatorName(OperatorName value)
    {
        return new TranslatedChanges(Reason, value, Organization,
            _changes,
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
            _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges WithOrganization(OrganizationId value)
    {
        return new TranslatedChanges(Reason, Operator, value,
            _changes,
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
            _modifyRoadSegmentProvisionalChanges);
    }

    public TranslatedChanges WithReason(Reason value)
    {
        return new TranslatedChanges(value, Operator, Organization,
            _changes,
            _addRoadSegmentChanges, _modifyRoadSegmentChanges, _removeRoadSegmentChanges,
            _modifyRoadSegmentProvisionalChanges);
    }

    public async Task<ChangeRoadNetwork> ToChangeRoadNetworkCommand(
        ILogger logger,
        ExtractRequestId extractRequestId,
        ChangeRequestId requestId,
        DownloadId downloadId,
        Guid? ticketId,
        CancellationToken cancellationToken)
    {
        var requestedChanges = new List<RequestedChange>();

        foreach (var change in this)
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            requestedChanges.Add(requestedChange);
        }

        var changeRoadNetwork = new ChangeRoadNetwork
        {
            ExtractRequestId = extractRequestId,
            RequestId = requestId,
            DownloadId = downloadId,
            Changes = requestedChanges.ToArray(),
            Reason = Reason,
            Operator = Operator,
            OrganizationId = Organization,
            TicketId = ticketId
        };

        var validator = new ChangeRoadNetworkValidator();
        var result = await validator.ValidateAsync(changeRoadNetwork, cancellationToken);

        if (!result.IsValid)
        {
            var zipArchiveProblems = result.Errors
                .Aggregate(
                    ZipArchiveProblems.None,
                    (current, error) => current.Add(new FileError(string.Empty, error.ErrorMessage)));
            var exception = new ZipArchiveValidationException(zipArchiveProblems);

            logger.LogError(exception, "BUG: ChangeRoadNetwork validation failed");

            throw exception;
        }

        return changeRoadNetwork;
    }
}
