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
        private readonly Func<RoadSegmentId, Func<AttributeId>> _nextRoadSegmentLaneAttributeId;
        private readonly Func<RoadSegmentId, Func<AttributeId>> _nextRoadSegmentWidthAttributeId;
        private readonly Func<RoadSegmentId, Func<AttributeId>> _nextRoadSegmentSurfaceAttributeId;

        public RequestedChangeTranslator(
            Func<RoadNodeId> nextRoadNodeId,
            Func<RoadSegmentId> nextRoadSegmentId,
            Func<AttributeId> nextEuropeanRoadAttributeId,
            Func<AttributeId> nextNationalRoadAttributeId,
            Func<AttributeId> nextNumberedRoadAttributeId,
            Func<RoadSegmentId, Func<AttributeId>> nextRoadSegmentLaneAttributeId,
            Func<RoadSegmentId, Func<AttributeId>> nextRoadSegmentWidthAttributeId,
            Func<RoadSegmentId, Func<AttributeId>> nextRoadSegmentSurfaceAttributeId)
        {
            _nextRoadNodeId =
                nextRoadNodeId ?? throw new ArgumentNullException(nameof(nextRoadNodeId));
            _nextRoadSegmentId =
                nextRoadSegmentId ?? throw new ArgumentNullException(nameof(nextRoadSegmentId));
            _nextEuropeanRoadAttributeId =
                nextEuropeanRoadAttributeId ?? throw new ArgumentNullException(nameof(nextEuropeanRoadAttributeId));
            _nextNationalRoadAttributeId =
                nextNationalRoadAttributeId ?? throw new ArgumentNullException(nameof(nextNationalRoadAttributeId));
            _nextNumberedRoadAttributeId =
                nextNumberedRoadAttributeId ?? throw new ArgumentNullException(nameof(nextNumberedRoadAttributeId));
            _nextRoadSegmentLaneAttributeId =
                nextRoadSegmentLaneAttributeId ?? throw new ArgumentNullException(nameof(nextRoadSegmentLaneAttributeId));
            _nextRoadSegmentWidthAttributeId =
                nextRoadSegmentWidthAttributeId ?? throw new ArgumentNullException(nameof(nextRoadSegmentWidthAttributeId));
            _nextRoadSegmentSurfaceAttributeId =
                nextRoadSegmentSurfaceAttributeId ?? throw new ArgumentNullException(nameof(nextRoadSegmentSurfaceAttributeId));
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
                    case Messages.AddRoadSegmentToEuropeanRoad command:
                        changeSet = changeSet.Append(Translate(command, changeSet));
                        break;
                    case Messages.AddRoadSegmentToNationalRoad command:
                        changeSet = changeSet.Append(Translate(command, changeSet));
                        break;
                    case Messages.AddRoadSegmentToNumberedRoad command:
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
            var nextLaneAttributeId = _nextRoadSegmentLaneAttributeId(permanent);
            var laneAttributes = Array.ConvertAll(
                command.Lanes,
                item => new RoadSegmentLaneAttribute(
                    nextLaneAttributeId(),
                    new AttributeId(item.AttributeId),
                    new RoadSegmentLaneCount(item.Count),
                    RoadSegmentLaneDirection.Parse(item.Direction),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var nextWidthAttributeId = _nextRoadSegmentWidthAttributeId(permanent);
            var widthAttributes = Array.ConvertAll(
                command.Widths,
                item => new RoadSegmentWidthAttribute(
                    nextWidthAttributeId(),
                    new AttributeId(item.AttributeId),
                    new RoadSegmentWidth(item.Width),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var nextSurfaceAttributeId = _nextRoadSegmentSurfaceAttributeId(permanent);
            var surfaceAttributes = Array.ConvertAll(
                command.Surfaces,
                item => new RoadSegmentSurfaceAttribute(
                    nextSurfaceAttributeId(),
                    new AttributeId(item.AttributeId),
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
                laneAttributes,
                widthAttributes,
                surfaceAttributes
            );
        }

        private AddRoadSegmentToEuropeanRoad Translate(Messages.AddRoadSegmentToEuropeanRoad command, IRequestedChanges requestedChanges)
        {
            var permanent = _nextEuropeanRoadAttributeId();
            var temporary = new AttributeId(command.TemporaryAttributeId);

            var segmentId = new RoadSegmentId(command.SegmentId);
            RoadSegmentId? temporarySegmentId;
            if (requestedChanges.TryResolvePermanent(segmentId, out var permanentSegmentId))
            {
                temporarySegmentId = segmentId;
                segmentId = permanentSegmentId;
            }
            else
            {
                temporarySegmentId = null;
            }

            var number = EuropeanRoadNumber.Parse(command.RoadNumber);
            return new AddRoadSegmentToEuropeanRoad
            (
                permanent,
                temporary,
                segmentId,
                temporarySegmentId,
                number
            );
        }

        private AddRoadSegmentToNationalRoad Translate(Messages.AddRoadSegmentToNationalRoad command, IRequestedChanges requestedChanges)
        {
            var permanent = _nextNationalRoadAttributeId();
            var temporary = new AttributeId(command.TemporaryAttributeId);

            var segmentId = new RoadSegmentId(command.SegmentId);
            RoadSegmentId? temporarySegmentId;
            if (requestedChanges.TryResolvePermanent(segmentId, out var permanentSegmentId))
            {
                temporarySegmentId = segmentId;
                segmentId = permanentSegmentId;
            }
            else
            {
                temporarySegmentId = null;
            }

            var number = NationalRoadNumber.Parse(command.Ident2);
            return new AddRoadSegmentToNationalRoad
            (
                permanent,
                temporary,
                segmentId,
                temporarySegmentId,
                number
            );
        }

        private AddRoadSegmentToNumberedRoad Translate(Messages.AddRoadSegmentToNumberedRoad command, IRequestedChanges requestedChanges)
        {
            var permanent = _nextNumberedRoadAttributeId();
            var temporary = new AttributeId(command.TemporaryAttributeId);

            var segmentId = new RoadSegmentId(command.SegmentId);
            RoadSegmentId? temporarySegmentId;
            if (requestedChanges.TryResolvePermanent(segmentId, out var permanentSegmentId))
            {
                temporarySegmentId = segmentId;
                segmentId = permanentSegmentId;
            }
            else
            {
                temporarySegmentId = null;
            }

            var number = NumberedRoadNumber.Parse(command.Ident8);
            var direction = RoadSegmentNumberedRoadDirection.Parse(command.Direction);
            var ordinal = new RoadSegmentNumberedRoadOrdinal(command.Ordinal);
            return new AddRoadSegmentToNumberedRoad
            (
                permanent,
                temporary,
                segmentId,
                temporarySegmentId,
                number,
                direction,
                ordinal
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
                typeof(Messages.AddRoadSegment),
                typeof(Messages.AddRoadSegmentToEuropeanRoad),
                typeof(Messages.AddRoadSegmentToNationalRoad),
                typeof(Messages.AddRoadSegmentToNumberedRoad)
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

            public RequestedChanges Append(AddRoadSegmentToEuropeanRoad change)
            {
                if (change == null)
                    throw new ArgumentNullException(nameof(change));

                return new RequestedChanges(
                    _changes.Add(change),
                    _mapToPermanentNodeIdentifiers,
                    _mapToTemporaryNodeIdentifiers,
                    _mapToPermanentSegmentIdentifiers,
                    _mapToTemporarySegmentIdentifiers);
            }

            public RequestedChanges Append(AddRoadSegmentToNationalRoad change)
            {
                if (change == null)
                    throw new ArgumentNullException(nameof(change));

                return new RequestedChanges(
                    _changes.Add(change),
                    _mapToPermanentNodeIdentifiers,
                    _mapToTemporaryNodeIdentifiers,
                    _mapToPermanentSegmentIdentifiers,
                    _mapToTemporarySegmentIdentifiers);
            }

            public RequestedChanges Append(AddRoadSegmentToNumberedRoad change)
            {
                if (change == null)
                    throw new ArgumentNullException(nameof(change));

                return new RequestedChanges(
                    _changes.Add(change),
                    _mapToPermanentNodeIdentifiers,
                    _mapToTemporaryNodeIdentifiers,
                    _mapToPermanentSegmentIdentifiers,
                    _mapToTemporarySegmentIdentifiers);
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
