namespace RoadRegistry.BackOffice.Schema.RoadSegmentLaneAttributes
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentLaneAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentLaneAttributeDbaseSchema Schema = new RoadSegmentLaneAttributeDbaseSchema();

        public RoadSegmentLaneAttributeDbaseRecord()
        {
            RS_OIDN = new DbaseInt32(Schema.RS_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            WS_GIDN = new DbaseString(Schema.WS_GIDN);
            AANTAL = new DbaseInt32(Schema.AANTAL);
            RICHTING = new DbaseInt32(Schema.RICHTING);
            LBLRICHT = new DbaseString(Schema.LBLRICHT);
            VANPOS = new DbaseDouble(Schema.VANPOS);
            TOTPOS = new DbaseDouble(Schema.TOTPOS);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseString(Schema.BEGINORG);
            LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

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

        public DbaseInt32 RS_OIDN { get; set; }
        public DbaseInt32 WS_OIDN { get; set; }
        public DbaseString WS_GIDN { get; set; }
        public DbaseInt32 AANTAL { get; set; }
        public DbaseInt32 RICHTING { get; set; }
        public DbaseString LBLRICHT { get; set; }
        public DbaseDouble VANPOS { get; set; }
        public DbaseDouble TOTPOS { get; set; }
        public DbaseDateTime BEGINTIJD { get; set; }
        public DbaseString BEGINORG { get; set; }
        public DbaseString LBLBGNORG { get; set; }
    }
}
