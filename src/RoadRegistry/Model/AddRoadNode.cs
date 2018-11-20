namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class AddRoadNode : IRequestedChange
    {
        public AddRoadNode(RoadNodeId id, RoadNodeType type, Point geometry)
        {
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public RoadNodeId Id { get; }
        public RoadNodeType Type { get; }
        public Point Geometry { get; }

        public Messages.AcceptedChange Accept()
        {
            return new Messages.AcceptedChange
            {
                RoadNodeAdded = new Messages.RoadNodeAdded
                {
                    Id = Id,
                    Type = Type.ToString(),
                    Geometry2 = new Messages.RoadNodeGeometry
                    {
                        SpatialReferenceSystemIdentifier = Geometry.SRID,
                        Point = new Messages.Point
                        {
                            X = Geometry.X,
                            Y = Geometry.Y
                        }
                    }
                }
            };
        }

        public Messages.RejectedChange Reject(IEnumerable<Messages.Reason> reasons)
        {
            return new Messages.RejectedChange
            {
                AddRoadNode = new Messages.AddRoadNode
                {
                    Id = Id,
                    Type = Type.ToString(),
                    Geometry2 = GeometryTranslator.Translate(Geometry)
                },
                Reasons = reasons.ToArray()
            };
        }
    }
}
