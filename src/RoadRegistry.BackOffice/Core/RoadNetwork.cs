namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Framework;
    using NetTopologySuite.Geometries;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<IRoadNetworkView, RoadNetwork> Factory =
            view => new RoadNetwork(view);

        private IRoadNetworkView _view;

        private RoadNetwork(IRoadNetworkView view)
        {
            _view = view;

            On<Messages.ImportedRoadNode>(e =>
            {
                _view = _view.RestoreFromEvent(e);
            });

            On<Messages.ImportedGradeSeparatedJunction>(e =>
            {
                _view = _view.RestoreFromEvent(e);
            });

            On<Messages.ImportedRoadSegment>(e =>
            {
                _view = _view.RestoreFromEvent(e);
            });

            On<Messages.RoadNetworkChangesAccepted>(e =>
            {
                _view = _view.RestoreFromEvent(e);
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
                Apply(new Messages.NoRoadNetworkChanges
                {
                    RequestId = requestId,
                    Reason = reason,
                    Operator = @operator,
                    OrganizationId = organization.Identifier,
                    Organization = organization.Name,
                    TransactionId = requestedChanges.TransactionId,
                });
            }
            else if (verifiedChanges.OfType<RejectedChange>().Any())
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

        public Func<RoadNodeId> ProvidesNextRoadNodeId()
        {
            return new NextRoadNodeIdProvider(_view.MaximumNodeId).Next;
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

        public Func<RoadSegmentId> ProvidesNextRoadSegmentId()
        {
            return new NextRoadSegmentIdProvider(_view.MaximumSegmentId).Next;
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

        public Func<GradeSeparatedJunctionId> ProvidesNextGradeSeparatedJunctionId()
        {
            return new NextGradeSeparatedJunctionIdProvider(_view.MaximumGradeSeparatedJunctionId).Next;
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

        private sealed class NextReusableAttributeIdProvider
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

        public Func<RoadSegmentId, RoadSegmentVersion> ProvidesNextRoadSegmentVersion()
        {
            return id =>
            {
                if (_view.Segments.TryGetValue(id, out var roadSegment) && roadSegment != null)
                {
                    return new NextRoadSegmentVersionProvider(roadSegment.Version).Next();
                }

                return new NextRoadSegmentVersionProvider(0).Next();
            };
        }
        private sealed class NextRoadSegmentVersionProvider
        {
            private RoadSegmentVersion _current;

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

        public Func<RoadSegmentId, MultiLineString, GeometryVersion> ProvidesNextRoadSegmentGeometryVersion()
        {
            return (id, geometry) =>
            {
                if (_view.Segments.TryGetValue(id, out var roadSegment) && roadSegment != null)
                {
                    if (roadSegment.Geometry != geometry)
                    {
                        return new NextRoadSegmentGeometryVersionProvider(roadSegment.GeometryVersion).Next();
                    }

                    return roadSegment.GeometryVersion;
                }

                return new NextRoadSegmentGeometryVersionProvider(0).Next();
            };
        }
        private sealed class NextRoadSegmentGeometryVersionProvider
        {
            private GeometryVersion _current;
            
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

        public void RestoreFromSnapshot(Messages.RoadNetworkSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            _view = ImmutableRoadNetworkView.Empty.RestoreFromSnapshot(snapshot);
        }

        public Messages.RoadNetworkSnapshot TakeSnapshot()
        {
            return _view.TakeSnapshot();
        }
    }
}
