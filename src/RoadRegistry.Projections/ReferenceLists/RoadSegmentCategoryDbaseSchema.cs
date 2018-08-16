namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentCategoryDbaseSchema : DbaseSchema
    {
        public RoadSegmentCategoryDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateStringField(
                    new DbaseFieldName(nameof(WEGCAT)),
                    new DbaseFieldLength(5)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLWEGCAT)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(DEFWEGCAT)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField WEGCAT => Fields[0];
        public DbaseField LBLWEGCAT => Fields[1];
        public DbaseField DEFWEGCAT => Fields[2];
    }
}
