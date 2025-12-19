namespace RoadRegistry.Extracts.Schemas.ExtractV1;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class TransactionZoneDbaseSchema : DbaseSchema
{
    public TransactionZoneDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(SOURCEID)),
                new DbaseFieldLength(5),
                new DbaseDecimalCount(0)),
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(TYPE)),
                new DbaseFieldLength(5),
                new DbaseDecimalCount(0)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(BESCHRIJV)),
                new DbaseFieldLength(254)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(OPERATOR)),
                new DbaseFieldLength(254)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(ORG)),
                new DbaseFieldLength(18)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(APPLICATIE)),
                new DbaseFieldLength(18)),
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(DOWNLOADID)),
                new DbaseFieldLength(32))
        };
    }

    public DbaseField APPLICATIE => Fields[5];
    public DbaseField BESCHRIJV => Fields[2];
    public DbaseField DOWNLOADID => Fields[6];
    public DbaseField OPERATOR => Fields[3];
    public DbaseField ORG => Fields[4];
    public DbaseField SOURCEID => Fields[0];
    public DbaseField TYPE => Fields[1];
}
