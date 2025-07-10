namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using Point = NetTopologySuite.Geometries.Point;

//TODO-pr make partial update
public class ModifyRoadNode : ITranslatedChange
{
    public ModifyRoadNode(RecordNumber recordNumber, RoadNodeId id, RoadNodeType type)
    {
        RecordNumber = recordNumber;
        Id = id;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Geometry = null;
    }

    private ModifyRoadNode(RecordNumber recordNumber, RoadNodeId id, RoadNodeType type, Point geometry)
    {
        RecordNumber = recordNumber;
        Id = id;
        Type = type;
        Geometry = geometry;
    }

    public Point Geometry { get; }
    public RoadNodeId Id { get; }
    public RecordNumber RecordNumber { get; }
    public RoadNodeType Type { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.ModifyRoadNode = new Messages.ModifyRoadNode
        {
            Id = Id,
            Type = Type.ToString(),
            Geometry = GeometryTranslator.Translate(Geometry)
        };
    }

    public ModifyRoadNode WithGeometry(Point geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        return new ModifyRoadNode(RecordNumber, Id, Type, geometry);
    }
}
