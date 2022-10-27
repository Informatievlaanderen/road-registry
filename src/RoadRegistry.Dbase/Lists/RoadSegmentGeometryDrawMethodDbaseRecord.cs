// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentGeometryDrawMethodDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentGeometryDrawMethodDbaseSchema Schema = new();

    public RoadSegmentGeometryDrawMethodDbaseRecord()
    {
        METHODE = new DbaseInt32(Schema.METHODE);
        LBLMETHOD = new DbaseString(Schema.LBLMETHOD);
        DEFMETHOD = new DbaseString(Schema.DEFMETHOD);

        Values = new DbaseFieldValue[]
        {
            METHODE, LBLMETHOD, DEFMETHOD
        };
    }

    public DbaseString DEFMETHOD { get; }
    public DbaseString LBLMETHOD { get; }
    public DbaseInt32 METHODE { get; }
}