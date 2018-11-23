namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
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
            _nextEuropeanRoadAttributeId = nextEuropeanRoadAttributeId ?? throw new ArgumentNullException(nameof(nextEuropeanRoadAttributeId));
            _nextNationalRoadAttributeId = nextNationalRoadAttributeId ?? throw new ArgumentNullException(nameof(nextNationalRoadAttributeId));
            _nextNumberedRoadAttributeId = nextNumberedRoadAttributeId ?? throw new ArgumentNullException(nameof(nextNumberedRoadAttributeId));
            _nextLaneAttributeId = nextLaneAttributeId ?? throw new ArgumentNullException(nameof(nextLaneAttributeId));
            _nextWidthAttributeId = nextWidthAttributeId ?? throw new ArgumentNullException(nameof(nextWidthAttributeId));
            _nextSurfaceAttributeId = nextSurfaceAttributeId ?? throw new ArgumentNullException(nameof(nextSurfaceAttributeId));
        }

        public IReadOnlyCollection<IRequestedChange> Translate(IReadOnlyCollection<Messages.RequestedChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var context = new TranslationContext();
            var translated = new List<IRequestedChange>(changes.Count);
            foreach (var change in changes.Flatten().OrderBy(_ => _, new RankChangeBeforeTranslation()))
            {
                switch (change)
                {
                    case Messages.AddRoadNode command:
                        {
                            var translation = Translate(command);
                            context.Map(translation);
                            translated.Add(translation);
                        }
                        break;
                    case Messages.AddRoadSegment command:
                        {
                            var translation = Translate(command, context);
                            context.Map(translation);
                            translated.Add(translation);
                        }
                        break;
                }
            }

            return translated;
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

        private AddRoadSegment Translate(Messages.AddRoadSegment command, ITranslationContext context)
        {
            var permanent = _nextRoadSegmentId();
            var temporary = new RoadSegmentId(command.TemporaryId);
            RoadNodeId startNodeId;
            RoadNodeId? temporaryStartNodeId;
            if (context.TryTranslate(new RoadNodeId(command.StartNodeId), out var permanentStartNodeId))
            {
                startNodeId = permanentStartNodeId;
                temporaryStartNodeId = new RoadNodeId(command.StartNodeId);
            }
            else
            {
                startNodeId = new RoadNodeId(command.StartNodeId);
                temporaryStartNodeId = null;
            }

            RoadNodeId endNodeId;
            RoadNodeId? temporaryEndNodeId;
            if (context.TryTranslate(new RoadNodeId(command.EndNodeId), out var permanentEndNodeId))
            {
                endNodeId = permanentEndNodeId;
                temporaryEndNodeId = new RoadNodeId(command.EndNodeId);
            }
            else
            {
                endNodeId = new RoadNodeId(command.EndNodeId);
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

        private class RankChangeBeforeTranslation : IComparer<object>
        {
            private static readonly Type[] SequenceByTypeOfChange =
            {
                typeof(Messages.AddRoadNode),
                typeof(Messages.AddRoadSegment)
            };

            public int Compare(object left, object right)
            {
                if (left == null) throw new ArgumentNullException(nameof(left));
                if (right == null) throw new ArgumentNullException(nameof(right));

                var leftRank = Array.IndexOf(SequenceByTypeOfChange, left.GetType());
                var rightRank = Array.IndexOf(SequenceByTypeOfChange, right.GetType());
                var comparison = leftRank.CompareTo(rightRank);
                if (comparison == 0)
                {
                    if (left is Messages.AddRoadNode leftNode &&
                        right is Messages.AddRoadNode rightNode)
                    {
                        return leftNode.TemporaryId.CompareTo(rightNode.TemporaryId);
                    }
                    if (left is Messages.AddRoadSegment leftSegment &&
                        right is Messages.AddRoadSegment rightSegment)
                    {
                        return leftSegment.TemporaryId.CompareTo(rightSegment.TemporaryId);
                    }
                }

                return comparison;
            }
        }

        private class TranslationContext : ITranslationContext
        {
            private readonly Dictionary<RoadNodeId, RoadNodeId> _mapOfNodes;
            private readonly Dictionary<RoadSegmentId, RoadSegmentId> _mapOfSegments;

            public TranslationContext()
            {
                _mapOfNodes = new Dictionary<RoadNodeId, RoadNodeId>();
                _mapOfSegments = new Dictionary<RoadSegmentId, RoadSegmentId>();
            }

            public void Map(AddRoadNode change) =>
                _mapOfNodes.Add(change.TemporaryId, change.Id);

            public bool TryTranslate(RoadNodeId id, out RoadNodeId translated) =>
                _mapOfNodes.TryGetValue(id, out translated);

            public RoadNodeId Translate(RoadNodeId id) =>
                _mapOfNodes.TryGetValue(id, out RoadNodeId permanent) ? permanent : id;

            public void Map(AddRoadSegment change) =>
                _mapOfSegments.Add(change.TemporaryId, change.Id);

            public bool TryTranslate(RoadSegmentId id, out RoadSegmentId translated) =>
                _mapOfSegments.TryGetValue(id, out translated);

            public RoadSegmentId Translate(RoadSegmentId id) =>
                _mapOfSegments.TryGetValue(id, out RoadSegmentId permanent) ? permanent : id;
        }

        private interface ITranslationContext
        {
            bool TryTranslate(RoadNodeId id, out RoadNodeId translated);
//            RoadNodeId Translate(RoadNodeId id);
//            bool TryTranslate(RoadSegmentId id, out RoadSegmentId translated);
//            RoadSegmentId Translate(RoadSegmentId id);
        }
    }
}
