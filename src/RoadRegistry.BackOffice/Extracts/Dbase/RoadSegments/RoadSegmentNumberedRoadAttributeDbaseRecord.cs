namespace RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNumberedRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentNumberedRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentNumberedRoadAttributeDbaseRecord()
    {
        GW_OIDN = new DbaseInt32(Schema.GW_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        IDENT8 = new TrimmedDbaseString(Schema.IDENT8);
        RICHTING = new DbaseInt32(Schema.RICHTING);
        LBLRICHT = new TrimmedDbaseString(Schema.LBLRICHT);
        VOLGNUMMER = new DbaseInt32(Schema.VOLGNUMMER);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            GW_OIDN,
            WS_OIDN,
            IDENT8,
            RICHTING,
            LBLRICHT,
            VOLGNUMMER,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }

    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseInt32 GW_OIDN { get; }
    public DbaseString IDENT8 { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseString LBLRICHT { get; }
    public DbaseInt32 RICHTING { get; }
    public DbaseInt32 VOLGNUMMER { get; }
    public DbaseInt32 WS_OIDN { get; }
}
