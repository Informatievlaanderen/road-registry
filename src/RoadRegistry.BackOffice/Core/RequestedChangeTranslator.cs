namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Messages;

    internal class RequestedChangeTranslator
    {
        private readonly Func<RoadNodeId> _nextRoadNodeId;
        private readonly Func<RoadSegmentId> _nextRoadSegmentId;
        private readonly Func<GradeSeparatedJunctionId> _nextGradeSeparatedJunctionId;
        private readonly Func<AttributeId> _nextEuropeanRoadAttributeId;
        private readonly Func<AttributeId> _nextNationalRoadAttributeId;
        private readonly Func<AttributeId> _nextNumberedRoadAttributeId;
        private readonly Func<RoadSegmentId, Func<AttributeId>> _nextRoadSegmentLaneAttributeId;
        private readonly Func<RoadSegmentId, Func<AttributeId>> _nextRoadSegmentWidthAttributeId;
        private readonly Func<RoadSegmentId, Func<AttributeId>> _nextRoadSegmentSurfaceAttributeId;

        public RequestedChangeTranslator(
            Func<RoadNodeId> nextRoadNodeId,
            Func<RoadSegmentId> nextRoadSegmentId,
            Func<GradeSeparatedJunctionId> nextGradeSeparatedJunctionId,
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
            _nextGradeSeparatedJunctionId =
                nextGradeSeparatedJunctionId ?? throw new ArgumentNullException(nameof(nextGradeSeparatedJunctionId));
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

        public RequestedChanges Translate(IReadOnlyCollection<Messages.RequestedChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var translated = RequestedChanges.Empty;
            foreach (var change in changes
                .Flatten()
                .Select((change, ordinal) => new SortableChange(change, ordinal))
                .OrderBy(_ => _, new RankChangeBeforeTranslation())
                .Select(_ => _.Change))
            {
                switch (change)
                {
                    case Messages.AddRoadNode command:
                        translated = translated.Append(Translate(command));
                        break;
                    case Messages.AddRoadSegment command:
                        translated = translated.Append(Translate(command, translated));
                        break;
                    case Messages.AddRoadSegmentToEuropeanRoad command:
                        translated = translated.Append(Translate(command, translated));
                        break;
                    case Messages.AddRoadSegmentToNationalRoad command:
                        translated = translated.Append(Translate(command, translated));
                        break;
                    case Messages.AddRoadSegmentToNumberedRoad command:
                        translated = translated.Append(Translate(command, translated));
                        break;
                    case Messages.AddGradeSeparatedJunction command:
                        translated = translated.Append(Translate(command, translated));
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

        private AddRoadSegment Translate(Messages.AddRoadSegment command, IRequestedChangeIdentityTranslator translator)
        {
            var permanent = _nextRoadSegmentId();
            var temporary = new RoadSegmentId(command.TemporaryId);

            var startNodeId = new RoadNodeId(command.StartNodeId);
            RoadNodeId? temporaryStartNodeId;
            if (translator.TryTranslateToPermanent(startNodeId, out var permanentStartNodeId))
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
            if (translator.TryTranslateToPermanent(endNodeId, out var permanentEndNodeId))
            {
                temporaryEndNodeId = endNodeId;
                endNodeId = permanentEndNodeId;
            }
            else
            {
                temporaryEndNodeId = null;
            }

            var geometry = GeometryTranslator.Translate(command.Geometry);
            var maintainer = new OrganizationId(command.MaintenanceAuthority);
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

        private AddRoadSegmentToEuropeanRoad Translate(Messages.AddRoadSegmentToEuropeanRoad command, IRequestedChangeIdentityTranslator translator)
        {
            var permanent = _nextEuropeanRoadAttributeId();
            var temporary = new AttributeId(command.TemporaryAttributeId);

            var segmentId = new RoadSegmentId(command.SegmentId);
            RoadSegmentId? temporarySegmentId;
            if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
            {
                temporarySegmentId = segmentId;
                segmentId = permanentSegmentId;
            }
            else
            {
                temporarySegmentId = null;
            }

            var number = EuropeanRoadNumber.Parse(command.Number);
            return new AddRoadSegmentToEuropeanRoad
            (
                permanent,
                temporary,
                segmentId,
                temporarySegmentId,
                number
            );
        }

        private AddRoadSegmentToNationalRoad Translate(Messages.AddRoadSegmentToNationalRoad command, IRequestedChangeIdentityTranslator translator)
        {
            var permanent = _nextNationalRoadAttributeId();
            var temporary = new AttributeId(command.TemporaryAttributeId);

            var segmentId = new RoadSegmentId(command.SegmentId);
            RoadSegmentId? temporarySegmentId;
            if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
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

        private AddRoadSegmentToNumberedRoad Translate(Messages.AddRoadSegmentToNumberedRoad command, IRequestedChangeIdentityTranslator translator)
        {
            var permanent = _nextNumberedRoadAttributeId();
            var temporary = new AttributeId(command.TemporaryAttributeId);

            var segmentId = new RoadSegmentId(command.SegmentId);
            RoadSegmentId? temporarySegmentId;
            if (translator.TryTranslateToPermanent(segmentId, out var permanentSegmentId))
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

        private AddGradeSeparatedJunction Translate(Messages.AddGradeSeparatedJunction command, IRequestedChangeIdentityTranslator translator)
        {
            var permanent = _nextGradeSeparatedJunctionId();
            var temporary = new GradeSeparatedJunctionId(command.TemporaryId);

            var upperSegmentId = new RoadSegmentId(command.UpperSegmentId);
            RoadSegmentId? temporaryUpperSegmentId;
            if (translator.TryTranslateToPermanent(upperSegmentId, out var permanentUpperSegmentId))
            {
                temporaryUpperSegmentId = upperSegmentId;
                upperSegmentId = permanentUpperSegmentId;
            }
            else
            {
                temporaryUpperSegmentId = null;
            }

            var lowerSegmentId = new RoadSegmentId(command.LowerSegmentId);
            RoadSegmentId? temporaryLowerSegmentId;
            if (translator.TryTranslateToPermanent(lowerSegmentId, out var permanentLowerSegmentId))
            {
                temporaryLowerSegmentId = lowerSegmentId;
                lowerSegmentId = permanentLowerSegmentId;
            }
            else
            {
                temporaryLowerSegmentId = null;
            }

            return new AddGradeSeparatedJunction(
                permanent,
                temporary,
                GradeSeparatedJunctionType.Parse(command.Type),
                upperSegmentId,
                temporaryUpperSegmentId,
                lowerSegmentId,
                temporaryLowerSegmentId);
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
                typeof(Messages.AddRoadSegmentToNumberedRoad),
                typeof(Messages.AddGradeSeparatedJunction)
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
    }
}
