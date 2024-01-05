namespace RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.V1.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentEuropeanRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentEuropeanRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentEuropeanRoadAttributeDbaseRecord()
    {
        EU_OIDN = new DbaseInt32(Schema.EU_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        EUNUMMER = new TrimmedDbaseString(Schema.EUNUMMER);

        Values = new DbaseFieldValue[]
        {
            EU_OIDN,
            WS_OIDN,
            EUNUMMER
        };
    }

    public DbaseInt32 EU_OIDN { get; }
    public DbaseString EUNUMMER { get; }
    public DbaseInt32 WS_OIDN { get; }
}
