namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using Point = NetTopologySuite.Geometries.Point;

public class AddRoadNode : ITranslatedChange
{
    public AddRoadNode(RecordNumber recordNumber, RoadNodeId temporaryId, RoadNodeType type)
    {
        RecordNumber = recordNumber;
        TemporaryId = temporaryId;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Geometry = null;
    }

    private AddRoadNode(RecordNumber recordNumber, RoadNodeId temporaryId, RoadNodeType type, Point geometry)
    {
        RecordNumber = recordNumber;
        TemporaryId = temporaryId;
        Type = type;
        Geometry = geometry;
    }

    public Point Geometry { get; }
    public RecordNumber RecordNumber { get; }
    public RoadNodeId TemporaryId { get; }
    public RoadNodeType Type { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadNode = new Messages.AddRoadNode
        {
            TemporaryId = TemporaryId,
            Type = Type.ToString(),
            Geometry = GeometryTranslator.Translate(Geometry)
        };
    }

    public AddRoadNode WithGeometry(Point geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        return new AddRoadNode(RecordNumber, TemporaryId, Type, geometry);
    }
}