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

            LBLMETHOD = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(LBLMETHOD)),
                    new DbaseFieldLength(64))
                .After(METHODE);

            DEFMETHOD = DbaseField
                .CreateStringField(
                    new DbaseFieldName(nameof(DEFMETHOD)),
                    new DbaseFieldLength(254))
                .After(LBLMETHOD);

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
