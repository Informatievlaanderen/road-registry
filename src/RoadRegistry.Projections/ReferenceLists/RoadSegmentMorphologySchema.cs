namespace RoadRegistry.Projections
{
    using Shaperon;

    public class RoadSegmentMorphologySchema
    {
        public RoadSegmentMorphologySchema()
        {
            MORF = DbaseField.CreateInt32Field(
                new DbaseFieldName(nameof(MORF)),
                new DbaseFieldLength(3));

            LBLMORF = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLMORF)),
                new DbaseFieldLength(64));

            DEFMORF = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(DEFMORF)),
                new DbaseFieldLength(254));
        }

        public DbaseField MORF { get; }
        public DbaseField LBLMORF { get; }
        public DbaseField DEFMORF { get; }
    }
}
