namespace RoadRegistry.BackOffice.Uploads.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class EuropeanRoadChangeDbaseSchema : DbaseSchema
{
    public EuropeanRoadChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(EU_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WS_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(EUNUMMER)),
                    new DbaseFieldLength(4)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TRANSACTID)),
                    new DbaseFieldLength(4),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RECORDTYPE)),
                    new DbaseFieldLength(4),
                    new DbaseDecimalCount(0))
        };
    }

    public DbaseField EU_OIDN => Fields[0];
    public DbaseField EUNUMMER => Fields[2];
    public DbaseField RECORDTYPE => Fields[4];
    public DbaseField TRANSACTID => Fields[3];
    public DbaseField WS_OIDN => Fields[1];
}
