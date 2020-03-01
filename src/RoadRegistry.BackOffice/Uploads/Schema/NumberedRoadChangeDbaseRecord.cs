namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NumberedRoadChangeDbaseRecord : DbaseRecord
    {
        public static readonly NumberedRoadChangeDbaseSchema Schema = new NumberedRoadChangeDbaseSchema();

        public NumberedRoadChangeDbaseRecord()
        {
            GW_OIDN = new DbaseNumber(Schema.GW_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            IDENT8 = new DbaseCharacter(Schema.IDENT8);
            RICHTING = new DbaseNumber(Schema.RICHTING);
            VOLGNUMMER = new DbaseNumber(Schema.VOLGNUMMER);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

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

        public DbaseNumber GW_OIDN { get; }

        public DbaseNumber WS_OIDN { get; }

        public DbaseCharacter IDENT8 { get; }

        public DbaseNumber RICHTING { get; }

        public DbaseNumber VOLGNUMMER { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
