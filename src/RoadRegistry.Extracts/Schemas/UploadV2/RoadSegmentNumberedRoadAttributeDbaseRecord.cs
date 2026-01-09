namespace RoadRegistry.Extracts.Schemas.UploadV2;

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
        VOLGNUMMER = new DbaseInt32(Schema.VOLGNUMMER);

        Values = new DbaseFieldValue[]
        {
            GW_OIDN,
            WS_OIDN,
            IDENT8,
            RICHTING,
            VOLGNUMMER,
        };
    }

    public DbaseInt32 GW_OIDN { get; }
    public DbaseString IDENT8 { get; }
    public DbaseInt32 RICHTING { get; }
    public DbaseInt32 VOLGNUMMER { get; }
    public DbaseInt32 WS_OIDN { get; }
}
