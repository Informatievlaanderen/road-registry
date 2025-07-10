namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using Point = NetTopologySuite.Geometries.Point;

public class ModifyRoadNode : ITranslatedChange
{
    public ModifyRoadNode(RecordNumber recordNumber, RoadNodeId id, RoadNodeType? type = null, Point? geometry = null)
    {
        RecordNumber = recordNumber;
        Id = id;
        Type = type;
        Geometry = geometry;
    }

    public RecordNumber RecordNumber { get; }
    public RoadNodeId Id { get; }
    public RoadNodeType? Type { get; }
    public Point? Geometry { get; }

    public void TranslateTo(RequestedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ModifyRoadNode = new Messages.ModifyRoadNode
        {
            Id = Id,
            Type = Type?.ToString(),
            Geometry = Geometry is not null ? GeometryTranslator.Translate(Geometry) : null
        };
    }

    public ModifyRoadNode WithGeometry(Point? geometry)
    {
        return new ModifyRoadNode(RecordNumber, Id, Type, geometry);
    }
}
