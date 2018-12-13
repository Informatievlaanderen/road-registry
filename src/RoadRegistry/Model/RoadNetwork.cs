namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework;
    using Messages;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
        public static readonly double TooCloseDistance = 2.0;

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

            On<RoadNetworkChangesAccepted>(e =>
            {
                _view = _view.Given(e);
            });
        }

        public void Change(RequestedChanges requestedChanges)
        {
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            var context = new ChangeContext(_view.With(requestedChanges), requestedChanges);
            requestedChanges
                .Aggregate(
                    VerifiedChanges.Empty,
                    (verifiedChanges, requestedChange) => verifiedChanges.Append(requestedChange.Verify(context)))
                .RecordUsing(Apply);
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
                if (_view.SegmentLaneAttributeIdentifiers.TryGetValue(id, out var recycledAttributeIdentifiers)
                    && recycledAttributeIdentifiers.Count != 0)
                {
                    return new NextRecycledAttributeIdProvider(provider, recycledAttributeIdentifiers).Next;
                }
                return provider.Next;
            };
        }

        public Func<RoadSegmentId, Func<AttributeId>> ProvidesNextRoadSegmentWidthAttributeId()
        {
            var provider = new NextAttributeIdProvider(_view.MaximumWidthAttributeId);
            return id =>
            {
                if (_view.SegmentWidthAttributeIdentifiers.TryGetValue(id, out var recycledAttributeIdentifiers)
                    && recycledAttributeIdentifiers.Count != 0)
                {
                    return new NextRecycledAttributeIdProvider(provider, recycledAttributeIdentifiers).Next;
                }

                return provider.Next;
            };
        }

        public Func<RoadSegmentId, Func<AttributeId>> ProvidesNextRoadSegmentSurfaceAttributeId()
        {
            var provider = new NextAttributeIdProvider(_view.MaximumSurfaceAttributeId);
            return id =>
            {
                if (_view.SegmentSurfaceAttributeIdentifiers.TryGetValue(id, out var recycledAttributeIdentifiers)
                    && recycledAttributeIdentifiers.Count != 0)
                {
                    return new NextRecycledAttributeIdProvider(provider, recycledAttributeIdentifiers).Next;
                }

                return provider.Next;
            };
        }

        private class NextRecycledAttributeIdProvider
        {
            private int _index;
            private readonly NextAttributeIdProvider _provider;
            private readonly IReadOnlyList<AttributeId> _recycledAttributeIdentifiers;

            public NextRecycledAttributeIdProvider(NextAttributeIdProvider provider, IReadOnlyList<AttributeId> recycledAttributeIdentifiers)
            {
                _provider = provider;
                _index = 0;
                _recycledAttributeIdentifiers = recycledAttributeIdentifiers;
            }

            public AttributeId Next()
            {
                return _index < _recycledAttributeIdentifiers.Count ? _recycledAttributeIdentifiers[_index++] : _provider.Next();
            }
        }
    }
}
