namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentChangeDbaseSchema Schema = new RoadSegmentChangeDbaseSchema();

        public RoadSegmentChangeDbaseRecord()
        {
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            METHODE = new DbaseNumber(Schema.METHODE);
            BEHEERDER = new DbaseCharacter(Schema.BEHEERDER);
            MORFOLOGIE = new DbaseNumber(Schema.MORFOLOGIE);
            STATUS = new DbaseNumber(Schema.STATUS);
            WEGCAT = new DbaseCharacter(Schema.WEGCAT);
            B_WK_OIDN = new DbaseNumber(Schema.B_WK_OIDN);
            E_WK_OIDN = new DbaseNumber(Schema.E_WK_OIDN);
            LSTRNMID = new DbaseNumber(Schema.LSTRNMID);
            RSTRNMID = new DbaseNumber(Schema.RSTRNMID);
            TGBEP = new DbaseNumber(Schema.TGBEP);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);
            EVENTIDN = new DbaseNumber(Schema.EVENTIDN);

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

        public DbaseNumber WS_OIDN { get; }

        public DbaseNumber METHODE { get; }

        public DbaseCharacter BEHEERDER { get; }

        public DbaseNumber MORFOLOGIE { get; }

        public DbaseNumber STATUS { get; }

        public DbaseCharacter WEGCAT { get; }

        public DbaseNumber B_WK_OIDN { get; }

        public DbaseNumber E_WK_OIDN { get; }

        public DbaseNumber LSTRNMID { get; }

        public DbaseNumber RSTRNMID { get; }

        public DbaseNumber TGBEP { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }

        public DbaseNumber EVENTIDN { get; }
    }
}
