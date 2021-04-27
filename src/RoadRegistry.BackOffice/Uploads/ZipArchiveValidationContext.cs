namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class ZipArchiveValidationContext
    {
        private readonly ImmutableHashSet<RoadSegmentId> _identicalSegments;
        private readonly ImmutableHashSet<RoadSegmentId> _addedSegments;
        private readonly ImmutableHashSet<RoadSegmentId> _modifiedSegments;
        private readonly ImmutableHashSet<RoadSegmentId> _removedSegments;

        public static readonly ZipArchiveValidationContext Empty = new ZipArchiveValidationContext(
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadSegmentId>.Empty);

        private ZipArchiveValidationContext(
            ImmutableHashSet<RoadSegmentId> identicalSegments,
            ImmutableHashSet<RoadSegmentId> addedSegments,
            ImmutableHashSet<RoadSegmentId> modifiedSegments,
            ImmutableHashSet<RoadSegmentId> removedSegments)
        {
            _identicalSegments = identicalSegments;
            _addedSegments = addedSegments;
            _modifiedSegments = modifiedSegments;
            _removedSegments = removedSegments;
        }

        public IImmutableSet<RoadSegmentId> KnownRoadSegments => _identicalSegments
            .Union(_addedSegments)
            .Union(_modifiedSegments)
            .Union(_removedSegments);

        public IImmutableSet<RoadSegmentId> KnownIdenticalRoadSegments => _identicalSegments;
        public IImmutableSet<RoadSegmentId> KnownAddedRoadSegments => _addedSegments;
        public IImmutableSet<RoadSegmentId> KnownModifiedRoadSegments => _modifiedSegments;
        public IImmutableSet<RoadSegmentId> KnownRemovedRoadSegments => _removedSegments;

        public bool Equals(ZipArchiveValidationContext other) =>
            other != null
            && _identicalSegments.SetEquals(other._identicalSegments)
            && _addedSegments.SetEquals(other._addedSegments)
            && _modifiedSegments.SetEquals(other._modifiedSegments)
            && _removedSegments.SetEquals(other._removedSegments);
        public override bool Equals(object obj) => obj is ZipArchiveValidationContext other && Equals(other);
        public override int GetHashCode() =>
            _identicalSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _addedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _modifiedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _removedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode());

        public ZipArchiveValidationContext WithIdenticalRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(_identicalSegments.Add(segment), _addedSegments, _modifiedSegments, _removedSegments);
        }

        public ZipArchiveValidationContext WithAddedRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(_identicalSegments, _addedSegments.Add(segment), _modifiedSegments, _removedSegments);
        }

        public ZipArchiveValidationContext WithModifiedRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(_identicalSegments, _addedSegments, _modifiedSegments.Add(segment), _removedSegments);
        }

        public ZipArchiveValidationContext WithRemovedRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(_identicalSegments, _addedSegments, _modifiedSegments, _removedSegments.Add(segment));
        }
    }
}
