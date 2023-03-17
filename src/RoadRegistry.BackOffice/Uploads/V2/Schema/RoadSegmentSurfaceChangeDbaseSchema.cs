namespace RoadRegistry.BackOffice.Uploads.Schema.V2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentSurfaceChangeDbaseSchema : DbaseSchema
{
    public RoadSegmentSurfaceChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WV_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15),
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
                    new DbaseFieldName(nameof(TYPE)),
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

    public DbaseField RECORDTYPE => Fields[6];
    public DbaseField TOTPOSITIE => Fields[3];
    public DbaseField TRANSACTID => Fields[5];
    public DbaseField TYPE => Fields[4];
    public DbaseField VANPOSITIE => Fields[2];
    public DbaseField WS_OIDN => Fields[1];
    public DbaseField WV_OIDN => Fields[0];
}
