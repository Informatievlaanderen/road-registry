namespace RoadRegistry.Projections
{
    using Shaperon;

    public class LaneDirectionDbaseSchema
    {
        public LaneDirectionDbaseSchema()
        {
            RICHTING = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(RICHTING)),
                new DbaseFieldLength(2));

            LBLRICHT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLRICHT)),
                new DbaseFieldLength(64));

            DEFRICHT = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(DEFRICHT)),
                new DbaseFieldLength(254));
        }

        public DbaseField RICHTING { get; }
        public DbaseField LBLRICHT { get;  }
        public DbaseField DEFRICHT { get; }
    }
}
