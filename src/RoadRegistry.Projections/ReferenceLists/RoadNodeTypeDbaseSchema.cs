namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadNodeTypeDbaseSchema : DbaseSchema
    {
        public RoadNodeTypeDbaseSchema()
        {
            TYPE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(TYPE)),
                new DbaseFieldLength(2));

            LBLTYPE = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLTYPE)),
                new DbaseFieldLength(64));

            DEFTYPE = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(DEFTYPE)),
                new DbaseFieldLength(254));

            Fields = new[]
            {
                TYPE,
                LBLTYPE,
                DEFTYPE
            };
        }

        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField DEFTYPE { get; }
    }
}
