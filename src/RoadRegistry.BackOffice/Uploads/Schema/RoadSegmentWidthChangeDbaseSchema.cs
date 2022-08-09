namespace RoadRegistry.BackOffice.Uploads.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentWidthChangeDbaseSchema : DbaseSchema
{
    public RoadSegmentWidthChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WB_OIDN)),
                new DbaseFieldLength(10),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(10),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VANPOSITIE)),
                    new DbaseFieldLength(8),
                    new DbaseDecimalCount(3)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TOTPOSITIE)),
                    new DbaseFieldLength(8),
                    new DbaseDecimalCount(3)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(BREEDTE)),
                    new DbaseFieldLength(4),
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

    public DbaseField WB_OIDN => Fields[0];

    public DbaseField WS_OIDN => Fields[1];

    public DbaseField VANPOSITIE => Fields[2];

    public DbaseField TOTPOSITIE => Fields[3];

    public DbaseField BREEDTE => Fields[4];

    public DbaseField TRANSACTID => Fields[5];

    public DbaseField RECORDTYPE => Fields[6];
}
