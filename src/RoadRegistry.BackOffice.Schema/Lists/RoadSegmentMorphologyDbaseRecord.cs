namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentMorphologyDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentMorphologyDbaseSchema Schema = new RoadSegmentMorphologyDbaseSchema();

        public RoadSegmentMorphologyDbaseRecord()
        {
            MORF = new DbaseNumber(Schema.MORF);
            LBLMORF = new DbaseCharacter(Schema.LBLMORF);
            DEFMORF = new DbaseCharacter(Schema.DEFMORF);

            Values = new DbaseFieldValue[]
            {
                MORF, LBLMORF, DEFMORF
            };
        }

        public DbaseNumber MORF { get; }
        public DbaseCharacter LBLMORF { get; }
        public DbaseCharacter DEFMORF { get; }
    }
}
