// ReSharper disable InconsistentNaming

namespace RoadRegistry.Dbase.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentStatusDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentStatusDbaseSchema Schema = new();

    public RoadSegmentStatusDbaseRecord()
    {
        STATUS = new DbaseInt32(Schema.STATUS);
        LBLSTATUS = new DbaseString(Schema.LBLSTATUS);
        DEFSTATUS = new DbaseString(Schema.DEFSTATUS);

        Values = new DbaseFieldValue[]
        {
            STATUS, LBLSTATUS, DEFSTATUS
        };
    }

    public DbaseString DEFSTATUS { get; }
    public DbaseString LBLSTATUS { get; }

    public DbaseInt32 STATUS { get; }
}