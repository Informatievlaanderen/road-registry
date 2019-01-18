namespace RoadRegistry.BackOfficeSchema.ReferenceData
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentStatusDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentStatusDbaseSchema Schema = new RoadSegmentStatusDbaseSchema();

        public RoadSegmentStatusDbaseRecord()
        {
            STATUS = new DbaseInt32(Schema.STATUS);
            LBLSTATUS = new DbaseString(Schema.LBLSTATUS);
            DEFSTATUS = new DbaseString(Schema.DEFSTATUS);

            Values = new DbaseFieldValue[]
            {
                STATUS, LBLSTATUS, DEFSTATUS
            };
        }

        public DbaseInt32 STATUS { get; }
        public DbaseString LBLSTATUS { get; }
        public DbaseString DEFSTATUS { get; }
    }
}
