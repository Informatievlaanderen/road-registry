namespace RoadRegistry.Extracts.Schemas.ExtractV2.RoadNodes;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseRecord : DbaseRecord
{
    public static readonly RoadNodeDbaseSchema Schema = new();

    public RoadNodeDbaseRecord()
    {
        WK_OIDN = new DbaseInt32(Schema.WK_OIDN);
        WK_UIDN = new TrimmedDbaseString(Schema.WK_UIDN);
        TYPE = new DbaseInt32(Schema.TYPE);
        LBLTYPE = new TrimmedDbaseString(Schema.LBLTYPE);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);

        Values =
        [
            WK_OIDN, WK_UIDN, TYPE, LBLTYPE, BEGINTIJD, BEGINORG
        ];
    }

    public DbaseInt32 WK_OIDN { get; }
    public DbaseString WK_UIDN { get; }
    public DbaseInt32 TYPE { get; }
    public DbaseString LBLTYPE { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseString BEGINORG { get; }
}
