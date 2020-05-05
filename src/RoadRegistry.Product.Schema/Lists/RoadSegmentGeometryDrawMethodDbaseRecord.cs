namespace RoadRegistry.Product.Schema.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentGeometryDrawMethodDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentGeometryDrawMethodDbaseSchema Schema = new RoadSegmentGeometryDrawMethodDbaseSchema();

        public RoadSegmentGeometryDrawMethodDbaseRecord()
        {
            METHODE = new DbaseInt32(Schema.METHODE);
            LBLMETHOD = new DbaseString(Schema.LBLMETHOD);
            DEFMETHOD = new DbaseString(Schema.DEFMETHOD);

            Values = new DbaseFieldValue[]
            {
                METHODE, LBLMETHOD, DEFMETHOD
            };
        }

        public DbaseInt32 METHODE { get; }
        public DbaseString LBLMETHOD { get; }
        public DbaseString DEFMETHOD { get; }
    }
}
