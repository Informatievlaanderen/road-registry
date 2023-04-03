namespace RoadRegistry.BackOffice.Uploads.Dbase.AfterFeatureCompare.V2.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionChangeDbaseSchema : DbaseSchema
{
    public GradeSeparatedJunctionChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(OK_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(BO_WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(ON_WS_OIDN)),
                    new DbaseFieldLength(15),
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

    public DbaseField BO_WS_OIDN => Fields[2];
    public DbaseField OK_OIDN => Fields[0];
    public DbaseField ON_WS_OIDN => Fields[3];
    public DbaseField RECORDTYPE => Fields[5];
    public DbaseField TRANSACTID => Fields[4];
    public DbaseField TYPE => Fields[1];
}
