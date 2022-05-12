// ReSharper disable InconsistentNaming
namespace RoadRegistry.Dbase.Lists
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class RoadSegmentGeometryDrawMethodDbaseSchema : DbaseSchema
    {
        public RoadSegmentGeometryDrawMethodDbaseSchema()
        {
            Fields = new[]
            {
                DbaseField.CreateNumberField(
                    new DbaseFieldName(nameof(METHODE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(LBLMETHOD)),
                        new DbaseFieldLength(64)),

                DbaseField
                    .CreateCharacterField(
                        new DbaseFieldName(nameof(DEFMETHOD)),
                        new DbaseFieldLength(254))
            };
        }

        public DbaseField METHODE => Fields[0];
        public DbaseField LBLMETHOD => Fields[1];
        public DbaseField DEFMETHOD => Fields[2];
    }
}
