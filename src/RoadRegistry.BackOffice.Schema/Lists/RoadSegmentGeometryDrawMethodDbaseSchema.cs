namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentGeometryDrawMethodDbaseSchema : DbaseSchema
    {
        public RoadSegmentGeometryDrawMethodDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(METHODE)),
                    new DbaseFieldLength(2)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLMETHOD)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(DEFMETHOD)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField METHODE => Fields[0];
        public DbaseField LBLMETHOD => Fields[1];
        public DbaseField DEFMETHOD => Fields[2];
    }
}
