namespace RoadRegistry.Extracts.Schemas.ExtractV2.RoadNodes;

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
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WK_UIDN)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINTIJD)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINORG)),
                    new DbaseFieldLength(18)),
        ];
    }

    public DbaseField WK_OIDN => Fields[0];
    public DbaseField TYPE => Fields[2];
    public DbaseField GRENSKNOOP => Fields[3];
    public DbaseField CREATIE => Fields[4];
    public DbaseField VERSIE => Fields[5];
}
