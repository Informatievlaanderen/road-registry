namespace RoadRegistry.BackOffice.Uploads.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadNodeChangeDbaseSchema : DbaseSchema
{
    public RoadNodeChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WEGKNOOPID)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(3),
                    new DbaseDecimalCount(0)),

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

    public DbaseField RECORDTYPE => Fields[3];
    public DbaseField TRANSACTID => Fields[2];
    public DbaseField TYPE => Fields[1];
    public DbaseField WEGKNOOPID => Fields[0];
}
