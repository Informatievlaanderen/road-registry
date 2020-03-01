namespace RoadRegistry.BackOffice.Schema.RoadSegmentLaneAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentLaneAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentLaneAttributeDbaseSchema Schema = new RoadSegmentLaneAttributeDbaseSchema();

        public RoadSegmentLaneAttributeDbaseRecord()
        {
            RS_OIDN = new DbaseNumber(Schema.RS_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            WS_GIDN = new DbaseCharacter(Schema.WS_GIDN);
            AANTAL = new DbaseNumber(Schema.AANTAL);
            RICHTING = new DbaseNumber(Schema.RICHTING);
            LBLRICHT = new DbaseCharacter(Schema.LBLRICHT);
            VANPOS = new DbaseNumber(Schema.VANPOS);
            TOTPOS = new DbaseNumber(Schema.TOTPOS);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                RS_OIDN,
                WS_OIDN,
                WS_GIDN,
                AANTAL,
                RICHTING,
                LBLRICHT,
                VANPOS,
                TOTPOS,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseNumber RS_OIDN { get; set; }
        public DbaseNumber WS_OIDN { get; set; }
        public DbaseCharacter WS_GIDN { get; set; }
        public DbaseNumber AANTAL { get; set; }
        public DbaseNumber RICHTING { get; set; }
        public DbaseCharacter LBLRICHT { get; set; }
        public DbaseNumber VANPOS { get; set; }
        public DbaseNumber TOTPOS { get; set; }
        public DbaseDateTime BEGINTIJD { get; set; }
        public DbaseCharacter BEGINORG { get; set; }
        public DbaseCharacter LBLBGNORG { get; set; }
    }
}
