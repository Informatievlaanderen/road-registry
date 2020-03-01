namespace RoadRegistry.BackOffice.Schema.RoadSegmentWidthAttributes
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentWidthAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentWidthAttributeDbaseSchema Schema = new RoadSegmentWidthAttributeDbaseSchema();

        public RoadSegmentWidthAttributeDbaseRecord()
        {
            WB_OIDN = new DbaseNumber(Schema.WB_OIDN);
            WS_OIDN = new DbaseNumber(Schema.WS_OIDN);
            WS_GIDN = new DbaseCharacter(Schema.WS_GIDN);
            BREEDTE = new DbaseNumber(Schema.BREEDTE);
            VANPOS = new DbaseNumber(Schema.VANPOS);
            TOTPOS = new DbaseNumber(Schema.TOTPOS);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseCharacter(Schema.BEGINORG);
            LBLBGNORG = new DbaseCharacter(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                WB_OIDN,
                WS_OIDN,
                WS_GIDN,
                BREEDTE,
                VANPOS,
                TOTPOS,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseNumber WB_OIDN { get; set; }
        public DbaseNumber WS_OIDN { get; set; }
        public DbaseCharacter WS_GIDN { get; set; }
        public DbaseNumber BREEDTE { get; set; }
        public DbaseNumber VANPOS { get; set; }
        public DbaseNumber TOTPOS { get; set; }
        public DbaseDateTime BEGINTIJD { get; set; }
        public DbaseCharacter BEGINORG { get; set; }
        public DbaseCharacter LBLBGNORG { get; set; }
    }
}
