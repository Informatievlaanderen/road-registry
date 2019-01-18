namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Aiv.Vbr.Shaperon;

    public class RoadNodeTypeDbaseRecord : DbaseRecord
    {
        private static readonly RoadNodeTypeDbaseSchema Schema = new RoadNodeTypeDbaseSchema();

        public RoadNodeTypeDbaseRecord()
        {
            TYPE = new DbaseInt32(Schema.TYPE);
            LBLTYPE = new DbaseString(Schema.LBLTYPE);
            DEFTYPE = new DbaseString(Schema.DEFTYPE);

            Values = new DbaseFieldValue[]
            {
                TYPE, LBLTYPE, DEFTYPE
            };
        }

        public DbaseInt32 TYPE { get; }
        public DbaseString LBLTYPE { get; }
        public DbaseString DEFTYPE { get; }
    }
}
