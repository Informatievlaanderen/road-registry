namespace RoadRegistry.BackOffice.Dbase.UploadsAfterFeatureCompare.V1.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class NumberedRoadChangeDbaseSchema : DbaseSchema
{
    public NumberedRoadChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(GW_OIDN)),
                new DbaseFieldLength(10),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(10),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(IDENT8)),
                    new DbaseFieldLength(8)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RICHTING)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VOLGNUMMER)),
                    new DbaseFieldLength(5),
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

    public DbaseField GW_OIDN => Fields[0];
    public DbaseField IDENT8 => Fields[2];
    public DbaseField RECORDTYPE => Fields[6];
    public DbaseField RICHTING => Fields[3];
    public DbaseField TRANSACTID => Fields[5];
    public DbaseField VOLGNUMMER => Fields[4];
    public DbaseField WS_OIDN => Fields[1];
}
