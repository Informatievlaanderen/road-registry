namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using RoadRegistry.Extensions;
using Point = NetTopologySuite.Geometries.Point;

public class AddRoadNode : ITranslatedChange
{
    public AddRoadNode(RecordNumber recordNumber, RoadNodeId temporaryId, RoadNodeId? originalId, RoadNodeType type)
        : this(recordNumber, temporaryId, originalId, type, null)
    {
    }

    private AddRoadNode(RecordNumber recordNumber, RoadNodeId temporaryId, RoadNodeId? originalId, RoadNodeType type, Point geometry)
    {
        RecordNumber = recordNumber;
        TemporaryId = temporaryId;
        OriginalId = originalId;
        Type = type.ThrowIfNull();
        Geometry = geometry;
    }

    public Point Geometry { get; }
    public RecordNumber RecordNumber { get; }
    public RoadNodeId TemporaryId { get; }
    public RoadNodeId? OriginalId { get; }
    public RoadNodeType Type { get; }

    public void TranslateTo(RequestedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.AddRoadNode = new Messages.AddRoadNode
        {
            TemporaryId = TemporaryId,
            OriginalId = OriginalId,
            Type = Type.ToString(),
            Geometry = GeometryTranslator.Translate(Geometry)
        };
    }

    public AddRoadNode WithGeometry(Point geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        return new AddRoadNode(RecordNumber, TemporaryId, OriginalId, Type, geometry);
    }
}
