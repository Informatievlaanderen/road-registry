namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();

        private RoadNetworkView _view;

        private RoadNetwork()
        {
            _view = RoadNetworkView.Empty;

            On<Messages.ImportedRoadNode>(e =>
            {
                _view = _view.Given(e);
            });

            On<Messages.ImportedGradeSeparatedJunction>(e =>
            {
                _view = _view.Given(e);
            });

            On<Messages.ImportedRoadSegment>(e =>
            {
                _view = _view.Given(e);
            });

            On<Messages.RoadNetworkChangesAccepted>(e =>
            {
                _view = _view.Given(e);
            });
        }

        public void Change(
            ChangeRequestId requestId,
            Reason reason,
            OperatorName @operator,
            Organization.DutchTranslation organization,
            RequestedChanges requestedChanges)
        {
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            var verifiedChanges = requestedChanges.VerifyWith(_view.With(requestedChanges));

            if (verifiedChanges.Count == 0) return;

            if (verifiedChanges.OfType<RejectedChange>().Any())
            {
                Apply(new Messages.RoadNetworkChangesRejected
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
            }
            else
            {
                Apply(new Messages.RoadNetworkChangesAccepted
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
        }

        public Func<TransactionId> ProvidesNextTransactionId()
        {
            return new NextTransactionIdProvider(_view.MaximumTransactionId).Next;
        }

        private class NextTransactionIdProvider
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

        public Func<RoadNodeId> ProvidesNextRoadNodeId()
        {
            return new NextRoadNodeIdProvider(_view.MaximumNodeId).Next;
        }

        private class NextRoadNodeIdProvider
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

        public Func<RoadSegmentId> ProvidesNextRoadSegmentId()
        {
            return new NextRoadSegmentIdProvider(_view.MaximumSegmentId).Next;
        }

        private class NextRoadSegmentIdProvider
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

        public Func<GradeSeparatedJunctionId> ProvidesNextGradeSeparatedJunctionId()
        {
            return new NextGradeSeparatedJunctionIdProvider(_view.MaximumGradeSeparatedJunctionId).Next;
        }

        private class NextGradeSeparatedJunctionIdProvider
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

        public Func<AttributeId> ProvidesNextEuropeanRoadAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumEuropeanRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextNationalRoadAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumNationalRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextNumberedRoadAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumNumberedRoadAttributeId).Next;
        }

        private class NextAttributeIdProvider
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

        private class NextReusableAttributeIdProvider
        {
            private int _index;
            private readonly NextAttributeIdProvider _provider;
            private readonly IReadOnlyList<AttributeId> _reusableAttributeIdentifiers;

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

        public void RestoreFromSnapshot(Messages.RoadNetworkSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            _view = RoadNetworkView.Empty.RestoreFromSnapshot(snapshot);
        }

        public Messages.RoadNetworkSnapshot TakeSnapshot()
        {
            return _view.TakeSnapshot();
        }
    }
}
