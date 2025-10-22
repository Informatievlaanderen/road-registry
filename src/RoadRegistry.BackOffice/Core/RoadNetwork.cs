namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

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
        On<RoadSegmentsStreetNamesChanged>(e => { _view = _view.RestoreFromEvent(e); });
    }

    public async Task<IMessage> Change(
        ChangeRequestId requestId,
        DownloadId? downloadId,
        Reason reason,
        OperatorName @operator,
        Organization.DutchTranslation organization,
        RequestedChanges requestedChanges,
        TicketId? ticketId,
        IExtractUploadFailedEmailClient emailClient,
        IOrganizations organizations,
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
            var afterContext = beforeContext.CreateAfterVerificationContext(_view.With(requestedChanges), organizations);

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
                DownloadId = downloadId,
                Reason = reason,
                Operator = @operator,
                OrganizationId = organization.Identifier,
                Organization = organization.Name,
                TransactionId = requestedChanges.TransactionId,
                TicketId = ticketId
            };
            Apply(@event);
            return @event;
        }

        if (verifiedChanges.OfType<RejectedChange>().Any())
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
                    .ToArray(),
                TicketId = ticketId
            };
            Apply(@event);

            if (downloadId is not null)
            {
                await emailClient.SendAsync(new (downloadId.Value, @event.Reason), cancellationToken);
            }
            return @event;
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
                    .SelectMany(change => change.Translate())
                    .ToArray(),
                TicketId = ticketId
            };
            Apply(@event);
            return @event;
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
        return FindRoadSegments(x => ids.Contains(x.Id));
    }

    public ICollection<RoadSegment> FindRoadSegments(Func<RoadSegment, bool> predicate)
    {
        return _view.Segments
            .Select(x => x.Value)
            .Where(predicate)
            .ToList();
    }

    public ICollection<RoadNode> FindRoadNodes(IEnumerable<RoadNodeId> ids)
    {
        return _view.Nodes
            .Select(x => x.Value)
            .Where(x => ids.Contains(x.Id))
            .ToList();
    }

    public IRoadNetworkIdProvider CreateIdProvider(IRoadNetworkIdGenerator idGenerator)
    {
        return new RoadNetworkIdProvider(idGenerator, _view);
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
}

public sealed record NextRoadSegmentVersionArgs(RoadSegmentId Id, RoadSegmentGeometryDrawMethod GeometryDrawMethod, bool ConvertedFromOutlined);
