namespace RoadRegistry.Extracts.Schemas.ExtractV2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class TransactionZoneDbaseSchema : DbaseSchema
{
    public TransactionZoneDbaseSchema()
    {
        Fields =
        [
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(TYPE)),
                new DbaseFieldLength(5),
                new DbaseDecimalCount(0)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(BESCHRIJV)),
                new DbaseFieldLength(254)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(DOWNLOADID)),
                new DbaseFieldLength(32))
        ];
    }

    public DbaseField TYPE => Fields[0];
    public DbaseField BESCHRIJV => Fields[1];
    public DbaseField DOWNLOADID => Fields[2];
}
