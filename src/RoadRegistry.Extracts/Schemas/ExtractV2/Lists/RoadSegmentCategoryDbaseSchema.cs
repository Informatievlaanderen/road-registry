// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.ExtractV2.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentCategoryDbaseSchema : DbaseSchema
{
    public RoadSegmentCategoryDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(WEGCAT)),
                new DbaseFieldLength(5)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLWEGCAT)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(DEFWEGCAT)),
                    new DbaseFieldLength(254))
        };
    }

    public DbaseField DEFWEGCAT => Fields[2];
    public DbaseField LBLWEGCAT => Fields[1];
    public DbaseField WEGCAT => Fields[0];
}
