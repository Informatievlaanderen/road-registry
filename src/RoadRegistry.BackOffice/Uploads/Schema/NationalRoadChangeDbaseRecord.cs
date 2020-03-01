namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class NationalRoadChangeDbaseRecord : DbaseRecord
    {
        public static readonly NationalRoadChangeDbaseSchema Schema = new NationalRoadChangeDbaseSchema();

        public NationalRoadChangeDbaseRecord()
        {
            NW_OIDN = new DbaseNumber(Schema.NW_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            IDENT2 = new DbaseCharacter(Schema.IDENT2);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                NW_OIDN,
                WS_OIDN,
                IDENT2,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber NW_OIDN { get; }

        public DbaseNumber WS_OIDN { get; }

        public DbaseCharacter IDENT2 { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
