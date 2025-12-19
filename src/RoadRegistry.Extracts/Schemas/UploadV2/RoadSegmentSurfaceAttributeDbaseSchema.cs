namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

public class RoadSegmentSurfaceAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentSurfaceAttributeDbaseSchema()
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
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VANPOS)),
                    new DbaseFieldLength(9),
                    new DbaseDecimalCount(3)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TOTPOS)),
                    new DbaseFieldLength(9),
                    new DbaseDecimalCount(3))
        };
    }

    public DbaseField TOTPOS => this.GetField();
    public DbaseField TYPE => this.GetField();
    public DbaseField VANPOS => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
    public DbaseField WV_OIDN => this.GetField();
}
