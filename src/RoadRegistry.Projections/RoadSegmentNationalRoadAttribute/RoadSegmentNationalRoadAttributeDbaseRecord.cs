namespace RoadRegistry.Projections
{
    using Shaperon;
    public class RoadSegmentNationalRoadAttributeDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentNationalRoadAttributeDbaseSchema Schema = new RoadSegmentNationalRoadAttributeDbaseSchema();

        public RoadSegmentNationalRoadAttributeDbaseRecord()
        {
            NW_OIDN = new DbaseInt32(Schema.NW_OIDN);
            WS_OIDN = new DbaseInt32(Schema.WS_OIDN);
            IDENT2 = new DbaseString(Schema.IDENT2);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseString(Schema.BEGINORG);
            LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                NW_OIDN,
                WS_OIDN,
                IDENT2,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseInt32 NW_OIDN { get; }
        public DbaseInt32 WS_OIDN { get; }
        public DbaseString IDENT2 { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseString BEGINORG { get; }
        public DbaseString LBLBGNORG { get; }
    }
}
