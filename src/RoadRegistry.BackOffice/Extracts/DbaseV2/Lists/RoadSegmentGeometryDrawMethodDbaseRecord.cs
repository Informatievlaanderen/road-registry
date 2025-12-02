// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Extracts.DbaseV2.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentGeometryDrawMethodDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentGeometryDrawMethodDbaseSchema Schema = new();

    public RoadSegmentGeometryDrawMethodDbaseRecord()
    {
        METHODE = new DbaseInt32(Schema.METHODE);
        LBLMETHOD = new TrimmedDbaseString(Schema.LBLMETHOD);
        DEFMETHOD = new TrimmedDbaseString(Schema.DEFMETHOD);

        Values = new DbaseFieldValue[]
        {
            METHODE, LBLMETHOD, DEFMETHOD
        };
    }

    public DbaseString DEFMETHOD { get; }
    public DbaseString LBLMETHOD { get; }
    public DbaseInt32 METHODE { get; }
}