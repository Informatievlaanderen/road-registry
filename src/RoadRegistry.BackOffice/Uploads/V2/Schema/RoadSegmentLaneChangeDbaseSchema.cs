namespace RoadRegistry.BackOffice.Uploads.V2.Schema;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class RoadSegmentLaneChangeDbaseSchema : DbaseSchema
{
    public RoadSegmentLaneChangeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(RS_OIDN)),
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
                    new DbaseFieldName(nameof(AANTAL)),
                    new DbaseFieldLength(4),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RICHTING)),
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

    public DbaseField AANTAL => Fields[4];
    public DbaseField RECORDTYPE => Fields[7];
    public DbaseField RICHTING => Fields[5];
    public DbaseField RS_OIDN => Fields[0];
    public DbaseField TOTPOSITIE => Fields[3];
    public DbaseField TRANSACTID => Fields[6];
    public DbaseField VANPOSITIE => Fields[2];
    public DbaseField WS_OIDN => Fields[1];
}
