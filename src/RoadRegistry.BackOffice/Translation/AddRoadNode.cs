namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using Model;
    using NetTopologySuite.Geometries;

    public class AddRoadNode : ITranslatedChange
    {
        public AddRoadNode(RoadNodeId temporaryId, RoadNodeType type)
        {
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = null;
        }

        private AddRoadNode(RoadNodeId temporaryId, RoadNodeType type, Point geometry)
        {
            TemporaryId = temporaryId;
            Type = type;
            Geometry = geometry;
        }

        public RoadNodeId TemporaryId { get; }
        public RoadNodeType Type { get; }
        public Point Geometry { get; }

        public AddRoadNode WithGeometry(Point geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            return new AddRoadNode(TemporaryId, Type, geometry);
        }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.AddRoadNode = new Messages.AddRoadNode
            {
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                Geometry = GeometryTranslator.Translate(Geometry)
            };
        }
    }
}
