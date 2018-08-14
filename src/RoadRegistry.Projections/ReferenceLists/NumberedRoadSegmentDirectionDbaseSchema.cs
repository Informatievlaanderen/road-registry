namespace RoadRegistry.Projections
{
    using Shaperon;

    public class NumberedRoadSegmentDirectionDbaseSchema : DbaseSchema
    {
        public NumberedRoadSegmentDirectionDbaseSchema()
        {
            RICHTING = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RICHTING)),
                new DbaseFieldLength(2));

            LBLRICHT = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLRICHT)),
                    new DbaseFieldLength(64))
                .After(RICHTING);

            DEFRICHT = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(DEFRICHT)),
                    new DbaseFieldLength(254))
                .After(LBLRICHT);

            Fields = new[]
            {
                RICHTING,
                LBLRICHT,
                DEFRICHT
            };
        }

        public DbaseField RICHTING { get; }
        public DbaseField LBLRICHT { get; }
        public DbaseField DEFRICHT { get; }
    }
}
