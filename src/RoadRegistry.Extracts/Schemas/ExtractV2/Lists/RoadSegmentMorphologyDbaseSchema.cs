// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV2.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentMorphologyDbaseSchema : DbaseSchema
{
    public RoadSegmentMorphologyDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(MORF)),
                new DbaseFieldLength(3),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLMORF)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(DEFMORF)),
                    new DbaseFieldLength(254))
        };
    }

    public DbaseField DEFMORF => Fields[2];
    public DbaseField LBLMORF => Fields[1];
    public DbaseField MORF => Fields[0];
}
