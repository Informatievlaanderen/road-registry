namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class NumberedRoadChangeDbaseRecord : DbaseRecord
{
    public static readonly NumberedRoadChangeDbaseSchema Schema = new();

    public NumberedRoadChangeDbaseRecord()
    {
        GW_OIDN = new DbaseInt32(Schema.GW_OIDN);
        WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
        IDENT8 = new TrimmedDbaseString(Schema.IDENT8);
        RICHTING = new DbaseInt16(Schema.RICHTING);
        VOLGNUMMER = new DbaseInt32(Schema.VOLGNUMMER);
        TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
        RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

        Values = new DbaseFieldValue[]
        {
            GW_OIDN,
            WS_OIDN,
            IDENT8,
            RICHTING,
            VOLGNUMMER,
            TRANSACTID,
            RECORDTYPE
        };
    }

    public DbaseInt32 GW_OIDN { get; }
    public DbaseString IDENT8 { get; }
    public DbaseInt16 RECORDTYPE { get; }
    public DbaseInt16 RICHTING { get; }
    public DbaseInt16 TRANSACTID { get; }
    public DbaseInt32 VOLGNUMMER { get; }
    public DbaseInt32 WS_OIDN { get; }
}
