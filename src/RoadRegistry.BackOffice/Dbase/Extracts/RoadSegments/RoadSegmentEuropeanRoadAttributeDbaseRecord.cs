namespace RoadRegistry.BackOffice.Dbase.Extracts.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentEuropeanRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentEuropeanRoadAttributeDbaseRecord()
    {
        EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        EUNUMMER = new DbaseString(Schema.EUNUMMER);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new DbaseString(Schema.BEGINORG);
        LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

        Values = new DbaseFieldValue[]
        {
            EU_OIDN,
            WS_OIDN,
            EUNUMMER,
            BEGINTIJD,
            BEGINORG,
            LBLBGNORG
        };
    }

    public DbaseString BEGINORG { get; }
    public DbaseDateTime BEGINTIJD { get; }
    public DbaseInt32 EU_OIDN { get; }
    public DbaseString EUNUMMER { get; }
    public DbaseString LBLBGNORG { get; }
    public DbaseInt32 WS_OIDN { get; }
}
