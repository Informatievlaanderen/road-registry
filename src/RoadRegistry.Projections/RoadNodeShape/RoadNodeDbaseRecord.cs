namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadNodeDbaseRecord : DbaseRecord
    {
        public static readonly RoadNodeDbaseSchema Schema = new RoadNodeDbaseSchema();

        public RoadNodeDbaseRecord()
        {
            WK_OIDN = new DbaseInt32(Schema.WK_OIDN);
            WK_UIDN = new DbaseString(Schema.WK_UIDN);
            TYPE = new DbaseInt32(Schema.WK_UIDN);
            LBLTYPE = new DbaseString(Schema.WK_UIDN);
            BEGINTIJD = new DbaseDateTime(Schema.WK_UIDN);
            BEGINORG = new DbaseString(Schema.WK_UIDN);
            LBLBGNORG = new DbaseString(Schema.WK_UIDN);

            Values = new DbaseFieldValue[]
            {
                WK_OIDN,WK_UIDN,TYPE,LBLTYPE,BEGINTIJD,BEGINORG,LBLBGNORG
            };
        }

        public DbaseInt32 WK_OIDN { get; }
        public DbaseString WK_UIDN { get; }
        public DbaseInt32 TYPE { get; }
        public DbaseString LBLTYPE { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseString BEGINORG { get; }
        public DbaseString LBLBGNORG { get; }
    }
}
