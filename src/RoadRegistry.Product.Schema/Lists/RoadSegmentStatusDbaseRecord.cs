namespace RoadRegistry.Product.Schema.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentStatusDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentStatusDbaseSchema Schema = new RoadSegmentStatusDbaseSchema();

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
