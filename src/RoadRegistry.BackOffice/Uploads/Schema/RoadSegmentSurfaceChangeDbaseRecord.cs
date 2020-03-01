namespace RoadRegistry.BackOffice.Uploads.Schema
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentSurfaceChangeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentSurfaceChangeDbaseSchema Schema = new RoadSegmentSurfaceChangeDbaseSchema();

        public RoadSegmentSurfaceChangeDbaseRecord()
        {
            WV_OIDN = new DbaseNumber(Schema.WV_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            VANPOSITIE = new DbaseNumber(Schema.VANPOSITIE);
            TOTPOSITIE = new DbaseNumber(Schema.TOTPOSITIE);
            TYPE = new DbaseNumber(Schema.TYPE);
            TRANSACTID = new DbaseNumber(Schema.TRANSACTID);
            RECORDTYPE = new DbaseNumber(Schema.RECORDTYPE);

            Values = new DbaseFieldValue[]
            {
                WV_OIDN,
                WS_OIDN,
                VANPOSITIE,
                TOTPOSITIE,
                TYPE,
                TRANSACTID,
                RECORDTYPE
            };
        }

        public DbaseNumber WV_OIDN { get; }

        public DbaseNumber WS_OIDN { get; }

        public DbaseNumber VANPOSITIE { get; }

        public DbaseNumber TOTPOSITIE { get; }

        public DbaseNumber TYPE { get; }

        public DbaseNumber TRANSACTID { get; }

        public DbaseNumber RECORDTYPE { get; }
    }
}
