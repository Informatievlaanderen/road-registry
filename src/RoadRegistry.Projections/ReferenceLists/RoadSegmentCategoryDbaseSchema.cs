namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentCategoryDbaseSchema
    {
        public RoadSegmentCategoryDbaseSchema()
        {
            WEGCAT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(WEGCAT)),
                new DbaseFieldLength(5));

            LBLWEGCAT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLWEGCAT)),
                new DbaseFieldLength(64));

            DEFWEGCAT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(DEFWEGCAT)),
                new DbaseFieldLength(254));
        }

        public DbaseField WEGCAT { get; }
        public DbaseField LBLWEGCAT { get; }
        public DbaseField DEFWEGCAT { get; }
    }
}
