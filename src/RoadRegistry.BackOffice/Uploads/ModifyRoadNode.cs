namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;

    public class ModifyRoadNode : ITranslatedChange
    {
        public ModifyRoadNode(RecordNumber recordNumber, RoadNodeId id, RoadNodeType type)
        {
            RecordNumber = recordNumber;
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = null;
        }

        private ModifyRoadNode(RecordNumber recordNumber, RoadNodeId id, RoadNodeType type, NetTopologySuite.Geometries.Point geometry)
        {
            RecordNumber = recordNumber;
            Id = id;
            Type = type;
            Geometry = geometry;
        }

        public RecordNumber RecordNumber { get; }
        public RoadNodeId Id { get; }
        public RoadNodeType Type { get; }
        public NetTopologySuite.Geometries.Point Geometry { get; }

        public ModifyRoadNode WithGeometry(NetTopologySuite.Geometries.Point geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            return new ModifyRoadNode(RecordNumber, Id, Type, geometry);
        }

        public void TranslateTo(Messages.RequestedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.ModifyRoadNode = new Messages.ModifyRoadNode
            {
                Id = Id,
                Type = Type.ToString(),
                Geometry = GeometryTranslator.Translate(Geometry)
            };
        }
    }
}
