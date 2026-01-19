namespace RoadRegistry.Extracts.Schemas.DomainV2.RoadNodes;

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
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(CREATIE)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(VERSIE)),
                    new DbaseFieldLength(15)),
        ];
    }

    public DbaseField WK_OIDN => Fields[0];
    public DbaseField TYPE => Fields[1];
    public DbaseField GRENSKNOOP => Fields[2];
    public DbaseField CREATIE => Fields[3];
    public DbaseField VERSIE => Fields[4];
}
