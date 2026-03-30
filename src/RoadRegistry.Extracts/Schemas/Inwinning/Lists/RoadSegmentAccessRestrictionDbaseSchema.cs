// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.Inwinning.Lists;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentAccessRestrictionDbaseSchema : DbaseSchema
{
    public RoadSegmentAccessRestrictionDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(TOEGANG)),
                new DbaseFieldLength(2),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTOEGANG)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(DEFTOEGANG)),
                    new DbaseFieldLength(254))
        };
    }

    public DbaseField DEFTOEGANG => Fields[2];
    public DbaseField LBLTOEGANG => Fields[1];
    public DbaseField TOEGANG => Fields[0];
}
