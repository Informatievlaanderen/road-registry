namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class EuropeanRoadChangeDbaseRecord : DbaseRecord
    {
        public static readonly EuropeanRoadChangeDbaseSchema Schema = new EuropeanRoadChangeDbaseSchema();

        public EuropeanRoadChangeDbaseRecord()
        {
            EU_OIDN = new DbaseNumber(Schema.EU_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            EUNUMMER = new DbaseCharacter(Schema.EUNUMMER);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                EU_OIDN,
                WS_OIDN,
                EUNUMMER,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber EU_OIDN { get; }

        public DbaseNumber WS_OIDN { get; }

        public DbaseCharacter EUNUMMER { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
