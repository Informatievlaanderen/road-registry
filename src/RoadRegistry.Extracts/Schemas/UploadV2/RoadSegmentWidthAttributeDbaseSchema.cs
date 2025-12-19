namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

public class RoadSegmentWidthAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentWidthAttributeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(WB_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(BREEDTE)),
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

    public DbaseField BREEDTE => this.GetField();
    public DbaseField TOTPOS => this.GetField();
    public DbaseField VANPOS => this.GetField();
    public DbaseField WB_OIDN => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
}
