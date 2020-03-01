namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentWidthChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentWidthChangeDbaseSchema Schema = new RoadSegmentWidthChangeDbaseSchema();

        public RoadSegmentWidthChangeDbaseRecord()
        {
            WB_OIDN = new DbaseNumber(Schema.WB_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            VANPOSITIE = new DbaseNumber(Schema.VANPOSITIE);
            TOTPOSITIE = new DbaseNumber(Schema.TOTPOSITIE);
            BREEDTE = new DbaseNumber(Schema.BREEDTE);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                WB_OIDN,
                WS_OIDN,
                VANPOSITIE,
                TOTPOSITIE,
                BREEDTE,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber WB_OIDN { get; }

        public DbaseNumber WS_OIDN { get; }

        public DbaseNumber VANPOSITIE { get; }

        public DbaseNumber TOTPOSITIE { get; }

        public DbaseNumber BREEDTE { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
