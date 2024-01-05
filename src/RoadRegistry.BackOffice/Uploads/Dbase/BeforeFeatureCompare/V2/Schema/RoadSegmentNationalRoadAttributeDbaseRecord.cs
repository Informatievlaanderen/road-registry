namespace RoadRegistry.BackOffice.Uploads.Dbase.BeforeFeatureCompare.V2.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentNationalRoadAttributeDbaseRecord : DbaseRecord
{
    public static readonly RoadSegmentNationalRoadAttributeDbaseSchema Schema = new();

    public RoadSegmentNationalRoadAttributeDbaseRecord()
    {
        NW_OIDN = new DbaseInt32(Schema.NW_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        IDENT2 = new TrimmedDbaseString(Schema.IDENT2);

        Values = new DbaseFieldValue[]
        {
            NW_OIDN,
            WS_OIDN,
            IDENT2
        };
    }

    public DbaseString IDENT2 { get; }
    public DbaseInt32 NW_OIDN { get; }
    public DbaseInt32 WS_OIDN { get; }
}
