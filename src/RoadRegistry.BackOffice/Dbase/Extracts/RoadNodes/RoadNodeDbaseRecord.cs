namespace RoadRegistry.BackOffice.Dbase.Extracts.RoadNodes;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeDbaseSchema Schema = new();

    public RoadNodeDbaseRecord()
    {
        WK_OIDN = new DbaseInt32(Schema.WK_OIDN);
        WK_UIDN = new DbaseString(Schema.WK_UIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new DbaseString(Schema.LBLTYPE);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            WK_OIDN, WK_UIDN, TYPE, LBLTYPE, BEGINTIJD, BEGINORG, LBLBGNORG
        };
    }

    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLTYPE { get; }
    public DbaseInt32 TYPE { get; }
    public DbaseInt32 WK_OIDN { get; }
    public DbaseString WK_UIDN { get; }
}