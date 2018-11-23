namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    public class RoadSegment
    {
        public RoadSegmentId Id { get; }
        public MultiLineString Geometry { get; }
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

        public RoadSegment(RoadSegmentId id, MultiLineString geometry, RoadNodeId start, RoadNodeId end)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            if (start == end)
                throw new ArgumentException("The start and end can not be the same road node.", nameof(start));

            Id = id;
            Geometry = geometry;
            Start = start;
            End = end;
        }

        public IEnumerable<RoadNodeId> SelectOppositeNode(RoadNodeId id)
        {
            if (Start == id)
            {
                yield return End;
            }
            else if (End == id)
            {
                yield return Start;
            }
        }

        //public KeyValuePair<RoadSegmentId, RoadSegment> ToKeyValuePair() => new KeyValuePair<RoadSegmentId, RoadSegment>(Id, this);
    }
}
