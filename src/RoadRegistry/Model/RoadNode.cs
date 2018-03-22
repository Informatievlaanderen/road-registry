namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    public class RoadNode
    {
        private readonly ImmutableHashSet<RoadLinkId> _links;

        public RoadNodeId Id { get; }

        public IReadOnlyCollection<RoadLinkId> Links => _links;

        public RoadNode(RoadNodeId id)
        {
            Id = id;
            _links = ImmutableHashSet<RoadLinkId>.Empty;
        }

        private RoadNode(RoadNodeId id, ImmutableHashSet<RoadLinkId> links)
        {
            Id = id;
            _links = links;
        }

        public RoadNode ConnectWith(RoadLinkId link)
        {
            return new RoadNode(Id, _links.Add(link));
        }

        public RoadNode DisconnectFrom(RoadLinkId link)
        {
            return new RoadNode(Id, _links.Remove(link));
        }

        //public KeyValuePair<RoadNodeId, RoadNode> ToKeyValuePair() => new KeyValuePair<RoadNodeId, RoadNode>(Id, this);

        // public Builder ToBuilder() => new Builder(this);

        // public class Builder
        // {
        //     private readonly ImmutableHashSet<RoadLinkId>.Builder _links;
        //     public RoadNodeId Id { get; }

        //     public IReadOnlyCollection<RoadLinkId> Links => _links;

        //     internal Builder(RoadNode node)
        //     {
        //         Id = node.Id;
        //         _links = node._links.ToBuilder();
        //     }

        //     public void ConnectWith(RoadLinkId link)
        //     {
        //         _links.Add(link);
        //     }

        //     public void DisconnectFrom(RoadLinkId link)
        //     {
        //         _links.Remove(link);
        //     }

        //     public RoadNode ToImmutable() => new RoadNode(Id, _links.ToImmutable());
        // }
    }
}