namespace RoadRegistry.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class RequestedChangeTranslator
    {
        private readonly Func<RoadNodeId> _nextRoadNodeId;
        private readonly Func<RoadSegmentId> _nextRoadSegmentId;
        private readonly Func<AttributeId> _nextEuropeanRoadAttributeId;
        private readonly Func<AttributeId> _nextNationalRoadAttributeId;
        private readonly Func<AttributeId> _nextNumberedRoadAttributeId;
        private readonly Func<AttributeId> _nextLaneAttributeId;
        private readonly Func<AttributeId> _nextWidthAttributeId;
        private readonly Func<AttributeId> _nextSurfaceAttributeId;

        public RequestedChangeTranslator(
            Func<RoadNodeId> nextRoadNodeId,
            Func<RoadSegmentId> nextRoadSegmentId,
            Func<AttributeId> nextEuropeanRoadAttributeId,
            Func<AttributeId> nextNationalRoadAttributeId,
            Func<AttributeId> nextNumberedRoadAttributeId,
            Func<AttributeId> nextLaneAttributeId,
            Func<AttributeId> nextWidthAttributeId,
            Func<AttributeId> nextSurfaceAttributeId)
        {
            _nextRoadNodeId = nextRoadNodeId ?? throw new ArgumentNullException(nameof(nextRoadNodeId));
            _nextRoadSegmentId = nextRoadSegmentId ?? throw new ArgumentNullException(nameof(nextRoadSegmentId));
            _nextEuropeanRoadAttributeId = nextEuropeanRoadAttributeId ??
                                           throw new ArgumentNullException(nameof(nextEuropeanRoadAttributeId));
            _nextNationalRoadAttributeId = nextNationalRoadAttributeId ??
                                           throw new ArgumentNullException(nameof(nextNationalRoadAttributeId));
            _nextNumberedRoadAttributeId = nextNumberedRoadAttributeId ??
                                           throw new ArgumentNullException(nameof(nextNumberedRoadAttributeId));
            _nextLaneAttributeId = nextLaneAttributeId ?? throw new ArgumentNullException(nameof(nextLaneAttributeId));
            _nextWidthAttributeId =
                nextWidthAttributeId ?? throw new ArgumentNullException(nameof(nextWidthAttributeId));
            _nextSurfaceAttributeId =
                nextSurfaceAttributeId ?? throw new ArgumentNullException(nameof(nextSurfaceAttributeId));
        }

        public IRequestedChanges Translate(IReadOnlyCollection<Messages.RequestedChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var changeSet = RequestedChanges.Empty;
            foreach (var change in changes
                .Flatten()
                .Select((change, ordinal) => new SortableChange(change, ordinal))
                .OrderBy(_ => _, new RankChangeBeforeTranslation())
                .Select(_ => _.Change))
            {
                switch (change)
                {
                    case Messages.AddRoadNode command:
                        changeSet = changeSet.Append(Translate(command));
                        break;
                    case Messages.AddRoadSegment command:
                        changeSet = changeSet.Append(Translate(command, changeSet));
                        break;
                }
            }

            return changeSet;
        }

        private AddRoadNode Translate(Messages.AddRoadNode command)
        {
            var permanent = _nextRoadNodeId();
            var temporary = new RoadNodeId(command.TemporaryId);
            return new AddRoadNode
            (
                permanent,
                temporary,
                RoadNodeType.Parse(command.Type),
                GeometryTranslator.Translate(command.Geometry)
            );
        }

        private AddRoadSegment Translate(Messages.AddRoadSegment command, IRequestedChanges requestedChanges)
        {
            var permanent = _nextRoadSegmentId();
            var temporary = new RoadSegmentId(command.TemporaryId);

            var startNodeId = new RoadNodeId(command.StartNodeId);
            RoadNodeId? temporaryStartNodeId;
            if (requestedChanges.TryResolvePermanent(startNodeId, out var permanentStartNodeId))
            {
                temporaryStartNodeId = startNodeId;
                startNodeId = permanentStartNodeId;
            }
            else
            {
                temporaryStartNodeId = null;
            }

            var endNodeId = new RoadNodeId(command.EndNodeId);
            RoadNodeId? temporaryEndNodeId;
            if (requestedChanges.TryResolvePermanent(endNodeId, out var permanentEndNodeId))
            {
                temporaryEndNodeId = endNodeId;
                endNodeId = permanentEndNodeId;
            }
            else
            {
                temporaryEndNodeId = null;
            }

            var geometry = GeometryTranslator.Translate(command.Geometry);
            var maintainer = new MaintenanceAuthorityId(command.MaintenanceAuthority);
            var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
            var morphology = RoadSegmentMorphology.Parse(command.Morphology);
            var status = RoadSegmentStatus.Parse(command.Status);
            var category = RoadSegmentCategory.Parse(command.Category);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(command.AccessRestriction);
            var leftSideStreetNameId = command.LeftSideStreetNameId.HasValue
                ? new CrabStreetnameId(command.LeftSideStreetNameId.Value)
                : new CrabStreetnameId?();
            var rightSideStreetNameId = command.RightSideStreetNameId.HasValue
                ? new CrabStreetnameId(command.RightSideStreetNameId.Value)
                : new CrabStreetnameId?();
            var partOfEuropeanRoads = Array.ConvertAll(
                command.PartOfEuropeanRoads,
                item => new RoadSegmentEuropeanRoadAttribute(
                    _nextEuropeanRoadAttributeId(),
                    EuropeanRoadNumber.Parse(item.RoadNumber)
                )
            );
            var partOfNationalRoads = Array.ConvertAll(
                command.PartOfNationalRoads,
                item => new RoadSegmentNationalRoadAttribute(
                    _nextNationalRoadAttributeId(),
                    NationalRoadNumber.Parse(item.Ident2)
                )
            );
            var partOfNumberedRoads = Array.ConvertAll(
                command.PartOfNumberedRoads,
                item => new RoadSegmentNumberedRoadAttribute(
                    _nextNumberedRoadAttributeId(),
                    NumberedRoadNumber.Parse(item.Ident8),
                    RoadSegmentNumberedRoadDirection.Parse(item.Direction),
                    new RoadSegmentNumberedRoadOrdinal(item.Ordinal)
                )
            );
            var laneAttributes = Array.ConvertAll(
                command.Lanes,
                item => new RoadSegmentLaneAttribute(
                    _nextLaneAttributeId(),
                    new RoadSegmentLaneCount(item.Count),
                    RoadSegmentLaneDirection.Parse(item.Direction),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var widthAttributes = Array.ConvertAll(
                command.Widths,
                item => new RoadSegmentWidthAttribute(
                    _nextWidthAttributeId(),
                    new RoadSegmentWidth(item.Width),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var surfaceAttributes = Array.ConvertAll(
                command.Surfaces,
                item => new RoadSegmentSurfaceAttribute(
                    _nextSurfaceAttributeId(),
                    RoadSegmentSurfaceType.Parse(item.Type),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );

            return new AddRoadSegment
            (
                permanent,
                temporary,
                startNodeId,
                temporaryStartNodeId,
                endNodeId,
                temporaryEndNodeId,
                geometry,
                maintainer,
                geometryDrawMethod,
                morphology,
                status,
                category,
                accessRestriction,
                leftSideStreetNameId,
                rightSideStreetNameId,
                partOfEuropeanRoads,
                partOfNationalRoads,
                partOfNumberedRoads,
                laneAttributes,
                widthAttributes,
                surfaceAttributes
            );
        }

        private class SortableChange
        {
            public int Ordinal { get; }
            public object Change { get; }

            public SortableChange(object change, int ordinal)
            {
                Ordinal = ordinal;
                Change = change;
            }
        }

        private class RankChangeBeforeTranslation : IComparer<SortableChange>
        {
            private static readonly Type[] SequenceByTypeOfChange =
            {
                typeof(Messages.AddRoadNode),
                typeof(Messages.AddRoadSegment)
            };

            public int Compare(SortableChange left, SortableChange right)
            {
                if (left == null) throw new ArgumentNullException(nameof(left));
                if (right == null) throw new ArgumentNullException(nameof(right));

                var leftRank = Array.IndexOf(SequenceByTypeOfChange, left.Change.GetType());
                var rightRank = Array.IndexOf(SequenceByTypeOfChange, right.Change.GetType());
                var comparison = leftRank.CompareTo(rightRank);
                return comparison != 0
                    ? comparison
                    : left.Ordinal.CompareTo(right.Ordinal);
            }
        }

        private class RequestedChanges : IRequestedChanges
        {
            public static readonly RequestedChanges Empty = new RequestedChanges(
                ImmutableList<IRequestedChange>.Empty,
                ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
                ImmutableDictionary<RoadNodeId, RoadNodeId>.Empty,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId>.Empty);

            private readonly ImmutableList<IRequestedChange> _changes;
            private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToPermanentNodeIdentifiers;
            private readonly ImmutableDictionary<RoadNodeId, RoadNodeId> _mapToTemporaryNodeIdentifiers;
            private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToPermanentSegmentIdentifiers;
            private readonly ImmutableDictionary<RoadSegmentId, RoadSegmentId> _mapToTemporarySegmentIdentifiers;

            private RequestedChanges(
                ImmutableList<IRequestedChange> changes,
                ImmutableDictionary<RoadNodeId, RoadNodeId> mapToPermanentNodeIdentifiers,
                ImmutableDictionary<RoadNodeId, RoadNodeId> mapToTemporaryNodeIdentifiers,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToPermanentSegmentIdentifiers,
                ImmutableDictionary<RoadSegmentId, RoadSegmentId> mapToTemporarySegmentIdentifiers)
            {
                _changes = changes;
                _mapToPermanentNodeIdentifiers = mapToPermanentNodeIdentifiers;
                _mapToTemporaryNodeIdentifiers = mapToTemporaryNodeIdentifiers;
                _mapToPermanentSegmentIdentifiers = mapToPermanentSegmentIdentifiers;
                _mapToTemporarySegmentIdentifiers = mapToTemporarySegmentIdentifiers;
            }

            public RequestedChanges Append(AddRoadNode change)
            {
                if (change == null)
                    throw new ArgumentNullException(nameof(change));

                return new RequestedChanges(
                    _changes.Add(change),
                    _mapToPermanentNodeIdentifiers.Add(change.TemporaryId, change.Id),
                    _mapToTemporaryNodeIdentifiers.Add(change.Id, change.TemporaryId),
                    _mapToPermanentSegmentIdentifiers,
                    _mapToTemporarySegmentIdentifiers);
            }

            public RequestedChanges Append(AddRoadSegment change)
            {
                if (change == null)
                    throw new ArgumentNullException(nameof(change));

                return new RequestedChanges(
                    _changes.Add(change),
                    _mapToPermanentNodeIdentifiers,
                    _mapToTemporaryNodeIdentifiers,
                    _mapToPermanentSegmentIdentifiers.Add(change.TemporaryId, change.Id),
                    _mapToTemporarySegmentIdentifiers.Add(change.Id, change.TemporaryId));
            }

            public bool TryResolvePermanent(RoadNodeId id, out RoadNodeId permanent)
            {
                return _mapToPermanentNodeIdentifiers.TryGetValue(id, out permanent);
            }

            public bool TryResolvePermanent(RoadSegmentId id, out RoadSegmentId permanent)
            {
                return _mapToPermanentSegmentIdentifiers.TryGetValue(id, out permanent);
            }

            public bool TryResolveTemporary(RoadNodeId id, out RoadNodeId temporary)
            {
                return _mapToTemporaryNodeIdentifiers.TryGetValue(id, out temporary);
            }

            public bool TryResolveTemporary(RoadSegmentId id, out RoadSegmentId temporary)
            {
                return _mapToTemporarySegmentIdentifiers.TryGetValue(id, out temporary);
            }

            public IEnumerator<IRequestedChange> GetEnumerator() =>
                ((IEnumerable<IRequestedChange>) _changes).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public int Count => _changes.Count;
        }
    }
}
