namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    
    public class RoadSegment
    {
        public RoadSegmentId Id { get; }
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

        public RoadSegment(RoadSegmentId id, RoadNodeId start, RoadNodeId end)
        {
            if(start == end)
            {
                throw new ArgumentException("The start and end can not be the same road node.", nameof(start));
            }

            Id = id;
            Start = start;
            End = end;
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

        //public KeyValuePair<RoadSegmentId, RoadSegment> ToKeyValuePair() => new KeyValuePair<RoadSegmentId, RoadSegment>(Id, this);
    }
}