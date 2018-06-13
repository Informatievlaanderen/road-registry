namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadReferencePointDbaseRecord : DbaseRecord
    {
        public static readonly RoadReferencePointDbaseSchema Schema = new RoadReferencePointDbaseSchema();

        public RoadReferencePointDbaseRecord()
        {
            RP_OIDN = new DbaseInt32(Schema.RP_OIDN);
            RP_UIDN = new DbaseString(Schema.RP_UIDN);
            IDENT8 = new DbaseString(Schema.IDENT8);
            OPSCHRIFT = new DbaseDouble(Schema.OPSCHRIFT);
            TYPE = new DbaseInt32(Schema.TYPE);
            LBLTYPE = new DbaseString(Schema.LBLTYPE);
            BEGINTIJD = new DbaseDateTime(Schema.BEGINTIJD);
            BEGINORG = new DbaseString(Schema.BEGINORG);
            LBLBGNORG = new DbaseString(Schema.LBLBGNORG);

            Values = new DbaseFieldValue[]
            {
                RP_OIDN,
                RP_UIDN,
                IDENT8,
                OPSCHRIFT,
                TYPE,
                LBLTYPE,
                BEGINTIJD,
                BEGINORG,
                LBLBGNORG,
            };
        }

        public DbaseInt32 RP_OIDN { get; }
        public DbaseString RP_UIDN { get; }
        public DbaseString IDENT8 { get; }
        public DbaseDouble OPSCHRIFT { get; }
        public DbaseInt32 TYPE { get; }
        public DbaseString LBLTYPE { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseString BEGINORG { get; }
        public DbaseString LBLBGNORG { get; }
    }
}
