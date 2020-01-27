namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentWidthChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentWidthChangeDbaseSchema Schema = new RoadSegmentWidthChangeDbaseSchema();

        public RoadSegmentWidthChangeDbaseRecord()
        {
            WB_OIDN = new DbaseInt32(Schema.WB_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            VANPOSITIE = new DbaseDouble(Schema.VANPOSITIE);
            TOTPOSITIE = new DbaseDouble(Schema.TOTPOSITIE);
            BREEDTE = new DbaseInt16(Schema.BREEDTE);
            TRANSACTID = new DbaseInt16(Schema.TRANSACTID);
            RECORDTYPE = new DbaseInt16(Schema.RECORDTYPE);

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

        public DbaseInt32 WB_OIDN { get; }

        public DbaseInt32 WS_OIDN { get; }

        public DbaseDouble VANPOSITIE { get; }

        public DbaseDouble TOTPOSITIE { get; }

        public DbaseInt16 BREEDTE { get; }

        public DbaseInt16 TRANSACTID { get; }

        public DbaseInt16 RECORDTYPE { get; }
    }
}
