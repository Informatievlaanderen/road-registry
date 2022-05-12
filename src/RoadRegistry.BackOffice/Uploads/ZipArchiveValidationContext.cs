namespace RoadRegistry.BackOffice.Uploads
{
    using System.Collections.Immutable;
    using System.Linq;

    public class ZipArchiveValidationContext
    {
        private readonly ImmutableHashSet<RoadSegmentId> _identicalSegments;
        private readonly ImmutableHashSet<RoadSegmentId> _addedSegments;
        private readonly ImmutableHashSet<RoadSegmentId> _modifiedSegments;
        private readonly ImmutableHashSet<RoadSegmentId> _removedSegments;

        private readonly ImmutableHashSet<RoadNodeId> _identicalNodes;
        private readonly ImmutableHashSet<RoadNodeId> _addedNodes;
        private readonly ImmutableHashSet<RoadNodeId> _modifiedNodes;
        private readonly ImmutableHashSet<RoadNodeId> _removedNodes;

        private readonly ZipArchiveMetadata _zipArchiveMetadata;

        public static readonly ZipArchiveValidationContext Empty = new ZipArchiveValidationContext(
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadSegmentId>.Empty,
            ImmutableHashSet<RoadNodeId>.Empty,
            ImmutableHashSet<RoadNodeId>.Empty,
            ImmutableHashSet<RoadNodeId>.Empty,
            ImmutableHashSet<RoadNodeId>.Empty,
            ZipArchiveMetadata.Empty);

        private ZipArchiveValidationContext(ImmutableHashSet<RoadSegmentId> identicalSegments,
            ImmutableHashSet<RoadSegmentId> addedSegments,
            ImmutableHashSet<RoadSegmentId> modifiedSegments,
            ImmutableHashSet<RoadSegmentId> removedSegments,
            ImmutableHashSet<RoadNodeId> identicalNodes,
            ImmutableHashSet<RoadNodeId> addedNodes,
            ImmutableHashSet<RoadNodeId> modifiedNodes,
            ImmutableHashSet<RoadNodeId> removedNodes,
            ZipArchiveMetadata zipArchiveMetadata)
        {
            _identicalSegments = identicalSegments;
            _addedSegments = addedSegments;
            _modifiedSegments = modifiedSegments;
            _removedSegments = removedSegments;
            _identicalNodes = identicalNodes;
            _addedNodes = addedNodes;
            _modifiedNodes = modifiedNodes;
            _removedNodes = removedNodes;
            _zipArchiveMetadata = zipArchiveMetadata;
        }

        public IImmutableSet<RoadSegmentId> KnownRoadSegments => _identicalSegments
            .Union(_addedSegments)
            .Union(_modifiedSegments)
            .Union(_removedSegments);

        public IImmutableSet<RoadNodeId> KnownRoadNodes => _identicalNodes
            .Union(_addedNodes)
            .Union(_modifiedNodes)
            .Union(_removedNodes);

        public IImmutableSet<RoadSegmentId> KnownIdenticalRoadSegments => _identicalSegments;
        public IImmutableSet<RoadSegmentId> KnownAddedRoadSegments => _addedSegments;
        public IImmutableSet<RoadSegmentId> KnownModifiedRoadSegments => _modifiedSegments;
        public IImmutableSet<RoadSegmentId> KnownRemovedRoadSegments => _removedSegments;

        public IImmutableSet<RoadNodeId> KnownIdenticalRoadNodes => _identicalNodes;
        public IImmutableSet<RoadNodeId> KnownAddedRoadNodes => _addedNodes;
        public IImmutableSet<RoadNodeId> KnownModifiedRoadNodes => _modifiedNodes;
        public IImmutableSet<RoadNodeId> KnownRemovedRoadNodes => _removedNodes;

        public ZipArchiveMetadata ZipArchiveMetadata => _zipArchiveMetadata;

        public bool Equals(ZipArchiveValidationContext other) =>
            other != null
            && _identicalSegments.SetEquals(other._identicalSegments)
            && _addedSegments.SetEquals(other._addedSegments)
            && _modifiedSegments.SetEquals(other._modifiedSegments)
            && _removedSegments.SetEquals(other._removedSegments)
            && _identicalNodes.SetEquals(other._identicalNodes)
            && _addedNodes.SetEquals(other._addedNodes)
            && _modifiedNodes.SetEquals(other._modifiedNodes)
            && _removedNodes.SetEquals(other._removedNodes)
            && _zipArchiveMetadata.Equals(other.ZipArchiveMetadata);
        public override bool Equals(object obj) => obj is ZipArchiveValidationContext other && Equals(other);
        public override int GetHashCode() =>
            _identicalSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _addedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _modifiedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _removedSegments.Aggregate(0, (current, segment) => current ^ segment.GetHashCode())
            ^ _identicalNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode())
            ^ _addedNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode())
            ^ _modifiedNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode())
            ^ _removedNodes.Aggregate(0, (current, node) => current ^ node.GetHashCode()
            ^ _zipArchiveMetadata.GetHashCode());

        public ZipArchiveValidationContext WithZipArchiveMetadata(ZipArchiveMetadata zipArchiveMetadata)
        {

            return new ZipArchiveValidationContext(_identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithIdenticalRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(
                _identicalSegments.Add(segment),
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithAddedRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(
                _identicalSegments,
                _addedSegments.Add(segment),
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithModifiedRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(_identicalSegments,
                _addedSegments,
                _modifiedSegments.Add(segment),
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithRemovedRoadSegment(RoadSegmentId segment)
        {
            return new ZipArchiveValidationContext(
                _identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments.Add(segment),
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithIdenticalRoadNode(RoadNodeId node)
        {
            return new ZipArchiveValidationContext(
                _identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes.Add(node),
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithAddedRoadNode(RoadNodeId node)
        {
            return new ZipArchiveValidationContext(
                _identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes.Add(node),
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithModifiedRoadNode(RoadNodeId node)
        {
            return new ZipArchiveValidationContext(_identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes.Add(node),
                _removedNodes,
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithRemovedRoadNode(RoadNodeId node)
        {
            return new ZipArchiveValidationContext(
                _identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes.Add(node),
                _zipArchiveMetadata);
        }

        public ZipArchiveValidationContext WithRoadNode(RoadNodeId node, RecordType recordType)
        {
            if (recordType == RecordType.Added)
            {
                return WithAddedRoadNode(node);
            }

            if (recordType == RecordType.Modified)
            {
                return WithModifiedRoadNode(node);
            }

            if (recordType == RecordType.Removed)
            {
                return WithRemovedRoadNode(node);
            }

            if (recordType == RecordType.Identical)
            {
                return WithIdenticalRoadNode(node);
            }

            return new ZipArchiveValidationContext(_identicalSegments,
                _addedSegments,
                _modifiedSegments,
                _removedSegments,
                _identicalNodes,
                _addedNodes,
                _modifiedNodes,
                _removedNodes,
                _zipArchiveMetadata);
        }
    }
}
