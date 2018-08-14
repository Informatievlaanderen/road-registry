namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentAccessRestrictionDbaseSchema : DbaseSchema
    {
        public RoadSegmentAccessRestrictionDbaseSchema()
        {
            TYPE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(TYPE)),
                new DbaseFieldLength(2));

            LBLTYPE = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64))
                .After(TYPE);

            DEFTYPE = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(DEFTYPE)),
                    new DbaseFieldLength(254))
                .After(LBLTYPE);

            Fields = new[]
            {
                TYPE,
                LBLTYPE,
                DEFTYPE,
            };
        }

        public DbaseField TYPE { get; }
        public DbaseField LBLTYPE { get; }
        public DbaseField DEFTYPE { get; }
    }
}
