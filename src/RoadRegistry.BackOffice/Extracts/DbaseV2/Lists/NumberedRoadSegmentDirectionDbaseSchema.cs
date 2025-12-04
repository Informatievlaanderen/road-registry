// ReSharper disable InconsistentNaming

namespace RoadRegistry.BackOffice.Extracts.DbaseV2.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class NumberedRoadSegmentDirectionDbaseSchema : DbaseSchema
{
    public NumberedRoadSegmentDirectionDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(RICHTING)),
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLRICHT)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(DEFRICHT)),
                    new DbaseFieldLength(254))
        };
    }

    public DbaseField DEFRICHT => Fields[2];
    public DbaseField LBLRICHT => Fields[1];
    public DbaseField RICHTING => Fields[0];
}