namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentCategoryDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentCategoryDbaseSchema Schema = new RoadSegmentCategoryDbaseSchema();

        public RoadSegmentCategoryDbaseRecord()
        {
            WEGCAT = new DbaseCharacter(Schema.WEGCAT);
            LBLWEGCAT = new DbaseCharacter(Schema.LBLWEGCAT);
            DEFWEGCAT = new DbaseCharacter(Schema.DEFWEGCAT);

            Values = new DbaseFieldValue[]
            {
                WEGCAT, LBLWEGCAT, DEFWEGCAT
            };
        }

        public DbaseCharacter WEGCAT { get; }
        public DbaseCharacter LBLWEGCAT { get; }
        public DbaseCharacter DEFWEGCAT { get; }
    }
}
