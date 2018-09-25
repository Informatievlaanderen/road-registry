namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;

    public class RoadSegmentMorphologyDbaseSchema : DbaseSchema
    {
        public RoadSegmentMorphologyDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateInt32Field(
                    new DbaseFieldName(nameof(MORF)),
                    new DbaseFieldLength(3)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLMORF)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(DEFMORF)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField MORF => Fields[0];
        public DbaseField LBLMORF => Fields[1];
        public DbaseField DEFMORF => Fields[2];
    }
}
