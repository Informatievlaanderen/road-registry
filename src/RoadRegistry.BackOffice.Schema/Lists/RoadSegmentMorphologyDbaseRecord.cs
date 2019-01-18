namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentMorphologyDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentMorphologyDbaseSchema Schema = new RoadSegmentMorphologyDbaseSchema();

        public RoadSegmentMorphologyDbaseRecord()
        {
            MORF = new DbaseInt32(Schema.MORF);
            LBLMORF = new DbaseString(Schema.LBLMORF);
            DEFMORF = new DbaseString(Schema.DEFMORF);

            Values = new DbaseFieldValue[]
            {
                MORF, LBLMORF, DEFMORF
            };
        }

        public DbaseInt32 MORF { get; }
        public DbaseString LBLMORF { get; }
        public DbaseString DEFMORF { get; }
    }
}
