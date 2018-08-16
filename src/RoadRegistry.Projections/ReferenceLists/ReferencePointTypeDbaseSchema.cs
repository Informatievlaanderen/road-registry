namespace RoadRegistry.Projections
{
    using Shaperon;

    public class ReferencePointTypeDbaseSchema : DbaseSchema
    {
        public ReferencePointTypeDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLTYPE)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(DEFTYPE)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField TYPE => Fields[0];
        public DbaseField LBLTYPE => Fields[1];
        public DbaseField DEFTYPE => Fields[2];
    }
}
