namespace RoadRegistry.BackOfficeSchema.ReferenceData
{
    using Aiv.Vbr.Shaperon;

    public class LaneDirectionDbaseRecord : DbaseRecord
    {
        private static readonly LaneDirectionDbaseSchema Schema = new LaneDirectionDbaseSchema();

        public LaneDirectionDbaseRecord()
        {
            RICHTING = new DbaseInt32(Schema.RICHTING);
            LBLRICHT = new DbaseString(Schema.LBLRICHT);
            DEFRICHT = new DbaseString(Schema.DEFRICHT);

            Values = new DbaseFieldValue[]
            {
                RICHTING, LBLRICHT, DEFRICHT
            };
        }

        public DbaseInt32 RICHTING { get; }
        public DbaseString LBLRICHT { get; }
        public DbaseString DEFRICHT { get; }
    }
}
