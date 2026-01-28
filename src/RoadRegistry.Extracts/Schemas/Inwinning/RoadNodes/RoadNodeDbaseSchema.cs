namespace RoadRegistry.Extracts.Schemas.Inwinning.RoadNodes;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeDbaseSchema : DbaseSchema
{
    public RoadNodeDbaseSchema()
    {
        Fields =
        [
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WK_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(GRENSKNOOP)),
                    new DbaseFieldLength(1),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(CREATIE)),
                    new DbaseFieldLength(15))
        ];
    }

    public DbaseField WK_OIDN => Fields[0];
    public DbaseField TYPE => Fields[1];
    public DbaseField GRENSKNOOP => Fields[2];
    public DbaseField CREATIE => Fields[3];
}
