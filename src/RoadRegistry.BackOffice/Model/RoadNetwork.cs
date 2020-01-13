namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Collections.Generic;
    using Framework;
    using Messages;
    using Translation;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();

        private RoadNetworkView _view;

        private RoadNetwork()
        {
            _view = RoadNetworkView.Empty;

            On<ImportedRoadNode>(e =>
            {
                _view = _view.Given(e);
            });

            On<ImportedGradeSeparatedJunction>(e =>
            {
                _view = _view.Given(e);
            });

            On<ImportedRoadSegment>(e =>
            {
                _view = _view.Given(e);
            });

            On<RoadNetworkChangesBasedOnArchiveAccepted>(e =>
            {
                _view = _view.Given(e);
            });
        }

        public void Change(RequestedChanges requestedChanges)
        {
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            requestedChanges
                .VerifyWith(_view.With(requestedChanges))
                .RecordUsing(Apply);
        }

        public void ChangeBaseOnArchive(ArchiveId archiveId, Reason reason, OperatorName @operator,
            OrganizationId organizationId, RequestedChanges requestedChanges)
        {
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            requestedChanges
                .VerifyWith(_view.With(requestedChanges))
                .RecordUsing(archiveId, reason, @operator, organizationId, Apply);
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

        public void RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            _view = RoadNetworkView.Empty.RestoreFromSnapshot(snapshot);
        }

        public RoadNetworkSnapshot TakeSnapshot()
        {
            return _view.TakeSnapshot();
        }
    }
}
