namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Framework;
using Messages;

public class RoadNetwork : EventSourcedEntity
{
    private RoadNetwork(IRoadNetworkView view)
    {
        _view = view;

        On<ImportedRoadNode>(e => { _view = _view.RestoreFromEvent(e); });
        On<ImportedGradeSeparatedJunction>(e => { _view = _view.RestoreFromEvent(e); });
        On<ImportedRoadSegment>(e => { _view = _view.RestoreFromEvent(e); });
        On<RoadNetworkChangesAccepted>(e => { _view = _view.RestoreFromEvent(e); });
    }

    private IRoadNetworkView _view;

    public void Change(
        ChangeRequestId requestId,
        Reason reason,
        OperatorName @operator,
        Organization.DutchTranslation organization,
        RequestedChanges requestedChanges)
    {
        var verifiableChanges =
            requestedChanges
                .Select(requestedChange => new VerifiableChange(requestedChange))
                .ToImmutableList();

        var beforeContext = requestedChanges.CreateBeforeVerificationContext(_view);
        foreach (var verifiableChange in verifiableChanges)
            verifiableChanges = verifiableChanges
                .Replace(verifiableChange, verifiableChange.VerifyBefore(beforeContext));

        if (!verifiableChanges.Any(change => change.HasErrors))
        {
            var afterContext = beforeContext.CreateAfterVerificationContext(_view.With(requestedChanges));
            foreach (var verifiableChange in verifiableChanges)
                verifiableChanges = verifiableChanges
                    .Replace(verifiableChange, verifiableChange.VerifyAfter(afterContext));
        }

        var verifiedChanges = verifiableChanges.ConvertAll(change => change.AsVerifiedChange());

        if (verifiedChanges.Count == 0)
            Apply(new NoRoadNetworkChanges
            {
                RequestId = requestId,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization.Identifier,
                Organization = organization.Name,
                TransactionId = requestedChanges.TransactionId
            });
        else if (verifiedChanges.OfType<RejectedChange>().Any())
            Apply(new RoadNetworkChangesRejected
            {
                RequestId = requestId,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization.Identifier,
                Organization = organization.Name,
                TransactionId = requestedChanges.TransactionId,
                Changes = verifiedChanges
                    .OfType<RejectedChange>()
                    .Select(change => change.Translate())
                    .ToArray()
            });
        else
            Apply(new RoadNetworkChangesAccepted
            {
                RequestId = requestId,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization.Identifier,
                Organization = organization.Name,
                TransactionId = requestedChanges.TransactionId,
                Changes = verifiedChanges
                    .OfType<AcceptedChange>()
                    .Select(change => change.Translate())
                    .ToArray()
            });
    }

    public static readonly Func<IRoadNetworkView, RoadNetwork> Factory =
        view => new RoadNetwork(view);

    private sealed class NextAttributeIdProvider
    {
        public NextAttributeIdProvider(AttributeId current)
        {
            _current = current;
        }

        private AttributeId _current;

        public AttributeId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextGradeSeparatedJunctionIdProvider
    {
        public NextGradeSeparatedJunctionIdProvider(GradeSeparatedJunctionId current)
        {
            _current = current;
        }

        private GradeSeparatedJunctionId _current;

        public GradeSeparatedJunctionId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextReusableAttributeIdProvider
    {
        public NextReusableAttributeIdProvider(NextAttributeIdProvider provider, IReadOnlyList<AttributeId> reusableAttributeIdentifiers)
        {
            _provider = provider;
            _index = 0;
            _reusableAttributeIdentifiers = reusableAttributeIdentifiers;
        }

        private int _index;
        private readonly NextAttributeIdProvider _provider;
        private readonly IReadOnlyList<AttributeId> _reusableAttributeIdentifiers;

        public AttributeId Next()
        {
            return _index < _reusableAttributeIdentifiers.Count ? _reusableAttributeIdentifiers[_index++] : _provider.Next();
        }
    }

    private sealed class NextRoadNodeIdProvider
    {
        public NextRoadNodeIdProvider(RoadNodeId current)
        {
            _current = current;
        }

        private RoadNodeId _current;

        public RoadNodeId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextRoadSegmentIdProvider
    {
        public NextRoadSegmentIdProvider(RoadSegmentId current)
        {
            _current = current;
        }

        private RoadSegmentId _current;

        public RoadSegmentId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextTransactionIdProvider
    {
        public NextTransactionIdProvider(TransactionId current)
        {
            _current = current;
        }

        private TransactionId _current;

        public TransactionId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    public Func<AttributeId> ProvidesNextEuropeanRoadAttributeId()
    {
        return new NextAttributeIdProvider(_view.MaximumEuropeanRoadAttributeId).Next;
    }

    public Func<GradeSeparatedJunctionId> ProvidesNextGradeSeparatedJunctionId()
    {
        return new NextGradeSeparatedJunctionIdProvider(_view.MaximumGradeSeparatedJunctionId).Next;
    }

    public Func<AttributeId> ProvidesNextNationalRoadAttributeId()
    {
        return new NextAttributeIdProvider(_view.MaximumNationalRoadAttributeId).Next;
    }

    public Func<AttributeId> ProvidesNextNumberedRoadAttributeId()
    {
        return new NextAttributeIdProvider(_view.MaximumNumberedRoadAttributeId).Next;
    }

    public Func<RoadNodeId> ProvidesNextRoadNodeId()
    {
        return new NextRoadNodeIdProvider(_view.MaximumNodeId).Next;
    }

    public Func<RoadSegmentId> ProvidesNextRoadSegmentId()
    {
        return new NextRoadSegmentIdProvider(_view.MaximumSegmentId).Next;
    }

    public Func<RoadSegmentId, Func<AttributeId>> ProvidesNextRoadSegmentLaneAttributeId()
    {
        var provider = new NextAttributeIdProvider(_view.MaximumLaneAttributeId);
        return id =>
        {
            if (_view.SegmentReusableLaneAttributeIdentifiers.TryGetValue(id, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
                return new NextReusableAttributeIdProvider(provider, reusableAttributeIdentifiers).Next;
            return provider.Next;
        };
    }

    public Func<RoadSegmentId, Func<AttributeId>> ProvidesNextRoadSegmentSurfaceAttributeId()
    {
        var provider = new NextAttributeIdProvider(_view.MaximumSurfaceAttributeId);
        return id =>
        {
            if (_view.SegmentReusableSurfaceAttributeIdentifiers.TryGetValue(id, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
                return new NextReusableAttributeIdProvider(provider, reusableAttributeIdentifiers).Next;

            return provider.Next;
        };
    }

    public Func<RoadSegmentId, Func<AttributeId>> ProvidesNextRoadSegmentWidthAttributeId()
    {
        var provider = new NextAttributeIdProvider(_view.MaximumWidthAttributeId);
        return id =>
        {
            if (_view.SegmentReusableWidthAttributeIdentifiers.TryGetValue(id, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
                return new NextReusableAttributeIdProvider(provider, reusableAttributeIdentifiers).Next;

            return provider.Next;
        };
    }

    public Func<TransactionId> ProvidesNextTransactionId()
    {
        return new NextTransactionIdProvider(_view.MaximumTransactionId).Next;
    }

    public void RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        _view = ImmutableRoadNetworkView.Empty.RestoreFromSnapshot(snapshot);
    }

    public RoadNetworkSnapshot TakeSnapshot()
    {
        return _view.TakeSnapshot();
    }
}
