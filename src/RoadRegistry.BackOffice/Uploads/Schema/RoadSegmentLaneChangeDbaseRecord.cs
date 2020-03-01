namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentLaneChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentLaneChangeDbaseSchema Schema = new RoadSegmentLaneChangeDbaseSchema();

        public RoadSegmentLaneChangeDbaseRecord()
        {
            RS_OIDN = new DbaseNumber(Schema.RS_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            VANPOSITIE = new DbaseNumber(Schema.VANPOSITIE);
            TOTPOSITIE = new DbaseNumber(Schema.TOTPOSITIE);
            AANTAL = new DbaseNumber(Schema.AANTAL);
            RICHTING = new DbaseNumber(Schema.RICHTING);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                RS_OIDN,
                WS_OIDN,
                VANPOSITIE,
                TOTPOSITIE,
                AANTAL,
                RICHTING,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber RS_OIDN { get; }

        public DbaseNumber WS_OIDN { get; }

        public DbaseNumber VANPOSITIE { get; }

        public DbaseNumber TOTPOSITIE { get; }

        public DbaseNumber AANTAL { get; }

        public DbaseNumber RICHTING { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
