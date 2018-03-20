namespace RoadRegistry
{
    using System;
    using System.Collections.Generic;
    public class RoadSegment
    {
        public RoadSegmentId Id { get; }
        public RoadNodeId Source { get; }
        public RoadNodeId Target { get; }

        public IEnumerable<RoadNodeId> Nodes 
        {
            get 
            {
                yield return Source;
                yield return Target;
            }
        }

        public RoadSegment(RoadSegmentId id, RoadNodeId source, RoadNodeId target)
        {
            if(source == target)
            {
                throw new ArgumentException("The source and target can not be the same road node.", nameof(source));
            }

            Id = id;
            Source = source;
            Target = target;
        }

        public IEnumerable<RoadNodeId> SelectCounterNode(RoadNodeId id)
        {
            if(Source == id)
            {
                yield return Target;
            }
            else if(Target == id)
            {
                yield return Source;
            }
        }

        //public KeyValuePair<RoadSegmentId, RoadSegment> ToKeyValuePair() => new KeyValuePair<RoadSegmentId, RoadSegment>(Id, this);
    }
}