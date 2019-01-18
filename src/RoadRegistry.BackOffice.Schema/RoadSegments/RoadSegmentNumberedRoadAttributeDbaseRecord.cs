namespace RoadRegistry.BackOffice.Schema.RoadSegmentNumberedRoadAttributes
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentNumberedRoadAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentNumberedRoadAttributeDbaseSchema Schema = new RoadSegmentNumberedRoadAttributeDbaseSchema();

        public RoadSegmentNumberedRoadAttributeDbaseRecord()
        {
            GW_OIDN = new DbaseInt32(Schema.GW_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            IDENT8 = new DbaseString(Schema.IDENT8);
            RICHTING = new DbaseInt32(Schema.RICHTING);
            LBLRICHT = new DbaseString(Schema.LBLRICHT);
            VOLGNUMMER = new DbaseInt32(Schema.VOLGNUMMER);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseString(Schema.BEGINORG);
            LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                GW_OIDN,
                WS_OIDN,
                IDENT8,
                RICHTING,
                LBLRICHT,
                VOLGNUMMER,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseInt32 GW_OIDN { get; }
        public DbaseInt32 WS_OIDN { get; }
        public DbaseString IDENT8 { get; }
        public DbaseInt32 RICHTING { get; }
        public DbaseString LBLRICHT { get; }
        public DbaseInt32 VOLGNUMMER { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseString BEGINORG { get; }
        public DbaseString LBLBGNORG { get; }
    }
}
