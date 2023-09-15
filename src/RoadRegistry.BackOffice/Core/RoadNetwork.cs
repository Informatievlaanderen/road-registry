namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Framework;
using Messages;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

public class RoadNetwork : EventSourcedEntity
{
    public static readonly Func<IRoadNetworkView, RoadNetwork> Factory =
        view => new RoadNetwork(view);

    public const int Identifier = 0;

    private IRoadNetworkView _view;

    private RoadNetwork(IRoadNetworkView view)
    {
        _view = view;

        On<ImportedRoadNode>(e => { _view = _view.RestoreFromEvent(e); });
        On<ImportedGradeSeparatedJunction>(e => { _view = _view.RestoreFromEvent(e); });
        On<ImportedRoadSegment>(e => { _view = _view.RestoreFromEvent(e); });
        On<RoadNetworkChangesAccepted>(e => { _view = _view.RestoreFromEvent(e); });
    }

    public async Task Change(
        ChangeRequestId requestId,
        DownloadId? downloadId,
        Reason reason,
        OperatorName @operator,
        Organization.DutchTranslation organization,
        RequestedChanges requestedChanges,
        IExtractUploadFailedEmailClient emailClient,
        CancellationToken cancellationToken)
    {
        var verifiableChanges =
            requestedChanges
                .Select(requestedChange => new VerifiableChange(requestedChange))
                .ToImmutableList();

        var beforeContext = requestedChanges.CreateBeforeVerificationContext(_view);
        foreach (var verifiableChange in verifiableChanges)
        {
            verifiableChanges = verifiableChanges
                .Replace(verifiableChange, verifiableChange.VerifyBefore(beforeContext));
        }

        if (!verifiableChanges.Any(change => change.HasErrors))
        {
            var afterContext = beforeContext.CreateAfterVerificationContext(_view.With(requestedChanges));
            foreach (var verifiableChange in verifiableChanges)
            {
                verifiableChanges = verifiableChanges
                    .Replace(verifiableChange, verifiableChange.VerifyAfter(afterContext));
            }
        }

        var verifiedChanges = verifiableChanges.ConvertAll(change => change.AsVerifiedChange());

        if (verifiedChanges.Count == 0)
        {
            var @event = new NoRoadNetworkChanges
            {
                RequestId = requestId,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization.Identifier,
                Organization = organization.Name,
                TransactionId = requestedChanges.TransactionId
            };
            Apply(@event);
        }
        else if (verifiedChanges.OfType<RejectedChange>().Any())
        {
            var @event = new RoadNetworkChangesRejected
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
            };
            Apply(@event);

            await emailClient.SendAsync(@event.Reason, new ValidationException(JsonConvert.SerializeObject(@event, Formatting.Indented)), cancellationToken);
        }
        else
        {
            var @event = new RoadNetworkChangesAccepted
            {
                RequestId = requestId,
                DownloadId = downloadId,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization.Identifier,
                Organization = organization.Name,
                TransactionId = requestedChanges.TransactionId,
                Changes = verifiedChanges
                    .OfType<AcceptedChange>()
                    .Select(change => change.Translate())
                    .ToArray()
            };
            Apply(@event);
        }
    }

    public RoadSegment FindRoadSegment(RoadSegmentId id)
    {
        if (_view.Segments.TryGetValue(id, out var segment))
        {
            return segment;
        }
        
        return null;
    }

    public ICollection<RoadSegment> FindRoadSegments(IEnumerable<RoadSegmentId> ids)
    {
        return _view.Segments
            .Select(x => x.Value)
            .Where(x => ids.Contains(x.Id))
            .ToList();
    }

    public ICollection<RoadNode> FindRoadNodes(IEnumerable<RoadNodeId> ids)
    {
        return _view.Nodes
            .Select(x => x.Value)
            .Where(x => ids.Contains(x.Id))
            .ToList();
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

    public Func<RoadNodeId, RoadNodeVersion> ProvidesNextRoadNodeVersion()
    {
        return id =>
        {
            if (_view.Nodes.TryGetValue(id, out var roadNode) && roadNode != null)
            {
                return new NextRoadNodeVersionProvider(roadNode.Version == 0 ? RoadNodeVersion.Initial : roadNode.Version).Next();
            }

            return new NextRoadNodeVersionProvider().Next();
        };
    }

    public Func<RoadSegmentId, MultiLineString, GeometryVersion> ProvidesNextRoadSegmentGeometryVersion()
    {
        return (id, geometry) =>
        {
            if (_view.Segments.TryGetValue(id, out var roadSegment) && roadSegment != null)
            {
                if (roadSegment.Geometry != geometry)
                {
                    return new NextRoadSegmentGeometryVersionProvider(roadSegment.GeometryVersion == 0 ? GeometryVersion.Initial : roadSegment.GeometryVersion).Next();
                }

                return roadSegment.GeometryVersion;
            }

            return new NextRoadSegmentGeometryVersionProvider().Next();
        };
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
            {
                return new NextReusableAttributeIdProvider(provider, reusableAttributeIdentifiers).Next;
            }

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
            {
                return new NextReusableAttributeIdProvider(provider, reusableAttributeIdentifiers).Next;
            }

            return provider.Next;
        };
    }

    public Func<RoadSegmentId, RoadSegmentVersion> ProvidesNextRoadSegmentVersion()
    {
        return id =>
        {
            if (_view.Segments.TryGetValue(id, out var roadSegment) && roadSegment != null)
            {
                return new NextRoadSegmentVersionProvider(roadSegment.Version == 0 ? RoadSegmentVersion.Initial : roadSegment.Version).Next();
            }
            return new NextRoadSegmentVersionProvider().Next();
        };
    }

    public Func<RoadSegmentId, Func<AttributeId>> ProvidesNextRoadSegmentWidthAttributeId()
    {
        var provider = new NextAttributeIdProvider(_view.MaximumWidthAttributeId);
        return id =>
        {
            if (_view.SegmentReusableWidthAttributeIdentifiers.TryGetValue(id, out var reusableAttributeIdentifiers)
                && reusableAttributeIdentifiers.Count != 0)
            {
                return new NextReusableAttributeIdProvider(provider, reusableAttributeIdentifiers).Next;
            }

            return provider.Next;
        };
    }

    public Func<TransactionId> ProvidesNextTransactionId()
    {
        return new NextTransactionIdProvider(_view.MaximumTransactionId).Next;
    }

    public void RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
    {
        if (snapshot == null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        _view = ImmutableRoadNetworkView.Empty.RestoreFromSnapshot(snapshot);
    }

    public RoadNetworkSnapshot TakeSnapshot()
    {
        return _view.TakeSnapshot();
    }

    private sealed class NextRoadNodeVersionProvider
    {
        private RoadNodeVersion _current;

        public NextRoadNodeVersionProvider()
            : this(0)
        {
        }

        public NextRoadNodeVersionProvider(int current)
        {
            _current = new RoadNodeVersion(current);
        }

        public RoadNodeVersion Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextRoadSegmentVersionProvider
    {
        private RoadSegmentVersion _current;

        public NextRoadSegmentVersionProvider()
            : this(0)
        {
        }

        public NextRoadSegmentVersionProvider(int current)
        {
            _current = new RoadSegmentVersion(current);
        }

        public RoadSegmentVersion Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextRoadSegmentGeometryVersionProvider
    {
        private GeometryVersion _current;

        public NextRoadSegmentGeometryVersionProvider()
            : this(0)
        {
        }

        public NextRoadSegmentGeometryVersionProvider(int current)
        {
            _current = new GeometryVersion(current);
        }

        public GeometryVersion Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextAttributeIdProvider
    {
        private AttributeId _current;

        public NextAttributeIdProvider(AttributeId current)
        {
            _current = current;
        }

        public AttributeId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextGradeSeparatedJunctionIdProvider
    {
        private GradeSeparatedJunctionId _current;

        public NextGradeSeparatedJunctionIdProvider(GradeSeparatedJunctionId current)
        {
            _current = current;
        }

        public GradeSeparatedJunctionId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextReusableAttributeIdProvider
    {
        private readonly NextAttributeIdProvider _provider;
        private readonly IReadOnlyList<AttributeId> _reusableAttributeIdentifiers;
        private int _index;

        public NextReusableAttributeIdProvider(NextAttributeIdProvider provider, IReadOnlyList<AttributeId> reusableAttributeIdentifiers)
        {
            _provider = provider;
            _index = 0;
            _reusableAttributeIdentifiers = reusableAttributeIdentifiers;
        }

        public AttributeId Next()
        {
            return _index < _reusableAttributeIdentifiers.Count ? _reusableAttributeIdentifiers[_index++] : _provider.Next();
        }
    }

    private sealed class NextRoadNodeIdProvider
    {
        private RoadNodeId _current;

        public NextRoadNodeIdProvider(RoadNodeId current)
        {
            _current = current;
        }

        public RoadNodeId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextRoadSegmentIdProvider
    {
        private RoadSegmentId _current;

        public NextRoadSegmentIdProvider(RoadSegmentId current)
        {
            _current = current;
        }

        public RoadSegmentId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }

    private sealed class NextTransactionIdProvider
    {
        private TransactionId _current;

        public NextTransactionIdProvider(TransactionId current)
        {
            _current = current;
        }

        public TransactionId Next()
        {
            var next = _current.Next();
            _current = next;
            return next;
        }
    }
}
