namespace RoadRegistry.BackOffice.Schema.ReferenceData
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentGeometryDrawMethodDbaseRecord : DbaseRecord
    {
        public static readonly RoadSegmentGeometryDrawMethodDbaseSchema Schema = new RoadSegmentGeometryDrawMethodDbaseSchema();

        public RoadSegmentGeometryDrawMethodDbaseRecord()
        {
            METHODE = new DbaseNumber(Schema.METHODE);
            LBLMETHOD = new DbaseCharacter(Schema.LBLMETHOD);
            DEFMETHOD = new DbaseCharacter(Schema.DEFMETHOD);

            Values = new DbaseFieldValue[]
            {
                METHODE, LBLMETHOD, DEFMETHOD
            };
        }

        public DbaseNumber METHODE { get; }
        public DbaseCharacter LBLMETHOD { get; }
        public DbaseCharacter DEFMETHOD { get; }
    }
}
