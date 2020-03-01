namespace RoadRegistry.BackOffice.Schema.RoadSegmentSurfaceAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentSurfaceAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentSurfaceAttributeDbaseSchema Schema = new RoadSegmentSurfaceAttributeDbaseSchema();

        public RoadSegmentSurfaceAttributeDbaseRecord()
        {
            WV_OIDN = new DbaseNumber(Schema.WV_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            WS_GIDN = new DbaseCharacter(Schema.WS_GIDN);
            TYPE = new DbaseNumber(Schema.TYPE);
            LBLTYPE = new DbaseCharacter(Schema.LBLTYPE);
            VANPOS = new DbaseNumber(Schema.VANPOS);
            TOTPOS = new DbaseNumber(Schema.TOTPOS);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                WV_OIDN,
                WS_OIDN,
                WS_GIDN,
                TYPE,
                LBLTYPE,
                VANPOS,
                TOTPOS,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG
            };
        }

        public DbaseNumber WV_OIDN { get; set; }
        public DbaseNumber WS_OIDN { get; set; }
        public DbaseCharacter WS_GIDN { get; set; }
        public DbaseNumber TYPE { get; set; }
        public DbaseCharacter LBLTYPE { get; set; }
        public DbaseNumber VANPOS { get; set; }
        public DbaseNumber TOTPOS { get; set; }
        public DbaseDateTime BEGINTIJD { get; set; }
        public DbaseCharacter BEGINORG { get; set; }
        public DbaseCharacter LBLBGNORG { get; set; }
    }
}
