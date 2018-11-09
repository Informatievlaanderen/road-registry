namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aiv.Vbr.Shaperon;

    public class AddRoadNode : IRequestedChange
    {
        public AddRoadNode(RoadNodeId id, RoadNodeType type, PointM geometry)
        {
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public RoadNodeId Id { get; }
        public RoadNodeType Type { get; }
        public PointM Geometry { get; }

        public Messages.AcceptedChange Accept(WellKnownBinaryWriter writer)
        {
            return new Messages.AcceptedChange
            {
                RoadNodeAdded = new Messages.RoadNodeAdded
                {
                    Id = Id,
                    Type = Type.ToString(),
                    Geometry = writer.Write(Geometry)
                }
            };
        }

        public Messages.RejectedChange Reject(WellKnownBinaryWriter writer, IEnumerable<Messages.Reason> reasons)
        {
            return new Messages.RejectedChange
            {
                AddRoadNode = new Messages.AddRoadNode
                {
                    Id = Id,
                    Type = Type.ToString(),
                    Geometry = writer.Write(Geometry)
                },
                Reasons = reasons.ToArray()
            };
        }
    }
}
