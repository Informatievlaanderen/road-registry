namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentChangeDbaseSchema Schema = new RoadSegmentChangeDbaseSchema();

        public RoadSegmentChangeDbaseRecord()
        {
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            METHODE = new DbaseInt16(Schema.METHODE);
            BEHEERDER = new DbaseString(Schema.BEHEERDER);
            MORFOLOGIE = new DbaseInt16(Schema.MORFOLOGIE);
            STATUS = new DbaseInt32(Schema.STATUS);
            WEGCAT = new DbaseString(Schema.WEGCAT);
            B_WK_OIDN = new DbaseInt32(Schema.B_WK_OIDN);
            E_WK_OIDN = new DbaseInt32(Schema.E_WK_OIDN);
            LSTRNMID = new DbaseNullableInt32(Schema.LSTRNMID);
            RSTRNMID = new DbaseNullableInt32(Schema.RSTRNMID);
            TGBEP = new DbaseInt16(Schema.TGBEP);
            TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
            RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);
            EVENTIDN = new DbaseInt32(Schema.EVENTIDN);

            Values = new DbaseFieldValue[]
            {
                WS_OIDN,
                METHODE,
                BEHEERDER,
                MORFOLOGIE,
                STATUS,
                WEGCAT,
                B_WK_OIDN,
                E_WK_OIDN,
                LSTRNMID,
                RSTRNMID,
                TGBEP,
                TRANSACTID,
                RECORDTYPE,
                EVENTIDN
            };
        }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseInt16 METHODE { get; }

        public DbaseString BEHEERDER { get; }

        public DbaseInt16 MORFOLOGIE { get; }

        public DbaseInt32 STATUS { get; }

        public DbaseString WEGCAT { get; }

        public DbaseInt32 B_WK_OIDN { get; }

        public DbaseInt32 E_WK_OIDN { get; }

        public DbaseNullableInt32 LSTRNMID { get; }

        public DbaseNullableInt32 RSTRNMID { get; }

        public DbaseInt16 TGBEP { get; }

        public DbaseInt16 TRANSACTID { get; }

        public DbaseInt16 RECORDTYPE { get; }

        public DbaseInt32 EVENTIDN { get; }
    }
}
