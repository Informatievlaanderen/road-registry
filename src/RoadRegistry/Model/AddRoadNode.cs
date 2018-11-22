namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class AddRoadNode : IRequestedChange
    {
        public AddRoadNode(RoadNodeId id, RoadNodeId temporaryId, RoadNodeType type, Point geometry)
        {
            Id = id;
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public RoadNodeId Id { get; }
        public RoadNodeId TemporaryId { get; }
        public RoadNodeType Type { get; }
        public Point Geometry { get; }

        public Messages.AcceptedChange Accept()
        {
            return new Messages.AcceptedChange
            {
                RoadNodeAdded = new Messages.RoadNodeAdded
                {
                    Id = Id,
                    TemporaryId = TemporaryId,
                    Type = Type.ToString(),
                    Geometry = new Messages.RoadNodeGeometry
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
                    TemporaryId = TemporaryId,
                    Type = Type.ToString(),
                    Geometry = GeometryTranslator.Translate(Geometry)
                },
                Reasons = reasons.ToArray()
            };
        }
    }
}
