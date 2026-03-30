// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.Inwinning.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class SurfaceTypeDbaseSchema : DbaseSchema
{
    public SurfaceTypeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(VERHARDING)),
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLVERHARD)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(DEFVERHARD)),
                    new DbaseFieldLength(254))
        };
    }

    public DbaseField DEFVERHARD => Fields[2];
    public DbaseField LBLVERHARD => Fields[1];
    public DbaseField VERHARDING => Fields[0];
}
