namespace RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentEuropeanRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentEuropeanRoadAttributeDbaseRecord()
    {
        EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        EUNUMMER = new TrimmedDbaseString(Schema.EUNUMMER);
        BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
        BEGINORG = new TrimmedDbaseString(Schema.BEGINORG);
        LBLBGNORG = new TrimmedDbaseString(Schema.LBLBGNORG);

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
