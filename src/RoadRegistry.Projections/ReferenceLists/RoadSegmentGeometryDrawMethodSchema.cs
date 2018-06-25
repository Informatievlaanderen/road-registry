namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentGeometryDrawMethodSchema
    {
        public RoadSegmentGeometryDrawMethodSchema()
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
        }

        public DbaseField METHODE { get; }
        public DbaseField LBLMETHOD { get; }
        public DbaseField DEFMETHOD { get; }
    }
}
