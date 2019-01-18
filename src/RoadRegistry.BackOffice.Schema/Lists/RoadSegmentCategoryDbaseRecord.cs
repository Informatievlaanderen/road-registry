namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentCategoryDbaseRecord : DbaseRecord
    {
        private static readonly RoadSegmentCategoryDbaseSchema Schema = new RoadSegmentCategoryDbaseSchema();

        public RoadSegmentCategoryDbaseRecord()
        {
            WEGCAT = new DbaseString(Schema.WEGCAT);
            LBLWEGCAT = new DbaseString(Schema.LBLWEGCAT);
            DEFWEGCAT = new DbaseString(Schema.DEFWEGCAT);

            Values = new DbaseFieldValue[]
            {
                WEGCAT, LBLWEGCAT, DEFWEGCAT
            };
        }

        public DbaseString WEGCAT { get; }
        public DbaseString LBLWEGCAT { get; }
        public DbaseString DEFWEGCAT { get; }
    }
}
