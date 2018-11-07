namespace RoadRegistry.Model
{
    using System;
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

        public Messages.AcceptedChange Accept()
        {
            return new Messages.AcceptedChange
            {
                RoadNodeAdded = new Messages.RoadNodeAdded
                {
                    Id = Id,
                    Geometry = Geometry.ToBinary(),
                    Type = (Messages.RoadNodeType) (int) Type
                }
            };
        }
    }
}
