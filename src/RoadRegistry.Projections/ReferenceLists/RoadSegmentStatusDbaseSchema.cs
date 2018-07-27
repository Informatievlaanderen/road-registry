namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentStatusDbaseSchema : DbaseSchema
    {
        public RoadSegmentStatusDbaseSchema()
        {
            STATUS = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(STATUS)),
                new DbaseFieldLength(2));

            LBLSTATUS = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLSTATUS)),
                new DbaseFieldLength(64));

            DEFSTATUS = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(DEFSTATUS)),
                new DbaseFieldLength(254));

            Fields = new[]
            {
                STATUS,
                LBLSTATUS,
                DEFSTATUS
            };
        }

        public DbaseField STATUS { get; }
        public DbaseField LBLSTATUS { get; }
        public DbaseField DEFSTATUS { get; }
    }
}
