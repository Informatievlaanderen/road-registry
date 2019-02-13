namespace RoadRegistry.BackOffice.Translation
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
            LSTRNMID = new DbaseInt32(Schema.LSTRNMID);
            RSTRNMID = new DbaseInt32(Schema.RSTRNMID);
            TGBEP = new DbaseInt16(Schema.TGBEP);
            TransactID = new DbaseInt32(Schema.TransactID);
            RecordType = new DbaseInt32(Schema.RecordType);
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
                TransactID,
                RecordType,
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

        public DbaseInt32 LSTRNMID { get; }

        public DbaseInt32 RSTRNMID { get; }

        public DbaseInt16 TGBEP { get; }

        public DbaseInt32 TransactID { get; }

        public DbaseInt32 RecordType { get; }

        public DbaseInt32 EVENTIDN { get; }
    }
}
