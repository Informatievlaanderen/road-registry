namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    
    public class RoadLink
    {
        public RoadLinkId Id { get; }
        public RoadNodeId Start { get; }
        public RoadNodeId End { get; }

        public IEnumerable<RoadNodeId> Nodes 
        {
            get 
            {
                yield return Start;
                yield return End;
            }
        }

        public RoadLink(RoadLinkId id, RoadNodeId source, RoadNodeId target)
        {
            if(source == target)
            {
                throw new ArgumentException("The start and end can not be the same road node.", nameof(source));
            }

            Id = id;
            Start = source;
            End = target;
        }

        public IEnumerable<RoadNodeId> SelectCounterNode(RoadNodeId id)
        {
            if(Start == id)
            {
                yield return End;
            }
            else if(End == id)
            {
                yield return Start;
            }
        }

        //public KeyValuePair<RoadLinkId, RoadLink> ToKeyValuePair() => new KeyValuePair<RoadLinkId, RoadLink>(Id, this);
    }
}