namespace RoadRegistry.BackOffice.Uploads
{
    using System;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;

    public class AddRoadNode : ITranslatedChange
    {
        public AddRoadNode(RecordNumber recordNumber, RoadNodeId temporaryId, RoadNodeType type)
        {
            RecordNumber = recordNumber;
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = null;
        }

        private AddRoadNode(RecordNumber recordNumber, RoadNodeId temporaryId, RoadNodeType type, NetTopologySuite.Geometries.Point geometry)
        {
            RecordNumber = recordNumber;
            TemporaryId = temporaryId;
            Type = type;
            Geometry = geometry;
        }

        public RecordNumber RecordNumber { get; }
        public RoadNodeId TemporaryId { get; }
        public RoadNodeType Type { get; }
        public NetTopologySuite.Geometries.Point Geometry { get; }

        public AddRoadNode WithGeometry(NetTopologySuite.Geometries.Point geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));
            return new AddRoadNode(RecordNumber, TemporaryId, Type, geometry);
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
