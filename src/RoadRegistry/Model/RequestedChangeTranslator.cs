namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class RequestedChangeTranslator
    {
        private readonly Func<RoadNodeId> _nextRoadNodeId;
        private readonly Func<RoadSegmentId> _nextRoadSegmentId;

        public RequestedChangeTranslator(Func<RoadNodeId> nextRoadNodeId, Func<RoadSegmentId> nextRoadSegmentId)
        {
            _nextRoadNodeId = nextRoadNodeId ?? throw new ArgumentNullException(nameof(nextRoadNodeId));
            _nextRoadSegmentId = nextRoadSegmentId ?? throw new ArgumentNullException(nameof(nextRoadSegmentId));
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
                return leftRank.CompareTo(rightRank);
            }
        }

        private class TranslationContext : ITranslationContext
        {
            private Dictionary<RoadNodeId, RoadNodeId> _mapOfNodes;
            private Dictionary<RoadSegmentId, RoadSegmentId> _mapOfSegments;

            public TranslationContext()
            {
                _mapOfNodes = new Dictionary<RoadNodeId, RoadNodeId>();
                _mapOfSegments = new Dictionary<RoadSegmentId, RoadSegmentId>();
            }

            public void Map(AddRoadNode change) =>
                _mapOfNodes.Add(change.TemporaryId, change.Id);

            public RoadNodeId Translate(RoadNodeId id) =>
                _mapOfNodes.TryGetValue(id, out RoadNodeId permanent) ? permanent : id;

            public void Map(AddRoadSegment change) =>
                _mapOfSegments.Add(change.TemporaryId, change.Id);

            public RoadSegmentId Translate(RoadSegmentId id) =>
                _mapOfSegments.TryGetValue(id, out RoadSegmentId permanent) ? permanent : id;
        }

        private interface ITranslationContext
        {
            RoadNodeId Translate(RoadNodeId id);
            RoadSegmentId Translate(RoadSegmentId id);
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
            var startNode = context.Translate(new RoadNodeId(command.StartNodeId));
            var endNode = context.Translate(new RoadNodeId(command.EndNodeId));
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
                item => EuropeanRoadNumber.Parse(item.RoadNumber)
            );
            var partOfNationalRoads = Array.ConvertAll(
                command.PartOfNationalRoads,
                item => NationalRoadNumber.Parse(item.Ident2)
            );
            var partOfNumberedRoads = Array.ConvertAll(
                command.PartOfNumberedRoads,
                item => new RoadSegmentNumberedRoadAttribute(
                    NumberedRoadNumber.Parse(item.Ident8),
                    RoadSegmentNumberedRoadDirection.Parse(item.Direction),
                    new RoadSegmentNumberedRoadOrdinal(item.Ordinal)
                )
            );
            var laneAttributes = Array.ConvertAll(
                command.Lanes,
                item => new RoadSegmentLaneAttribute(
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
                    new RoadSegmentWidth(item.Width),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var surfaceAttributes = Array.ConvertAll(
                command.Surfaces,
                item => new RoadSegmentSurfaceAttribute(
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
                startNode,
                endNode,
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
    }
}
