namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentGeometryDrawMethodDbaseSchema : DbaseSchema
    {
        public RoadSegmentGeometryDrawMethodDbaseSchema()
        {
            METHODE = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(METHODE)),
                new DbaseFieldLength(2));

            LBLMETHOD = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLMETHOD)),
                new DbaseFieldLength(64));

            DEFMETHOD = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(DEFMETHOD)),
                new DbaseFieldLength(254));

            Fields = new[]
            {
                METHODE,
                LBLMETHOD,
                DEFMETHOD,
            };
        }

        public DbaseField METHODE { get; }
        public DbaseField LBLMETHOD { get; }
        public DbaseField DEFMETHOD { get; }
    }
}
