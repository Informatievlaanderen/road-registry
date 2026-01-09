namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

public class GradeSeparatedJunctionDbaseSchema : DbaseSchema
{
    public GradeSeparatedJunctionDbaseSchema()
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
                    new DbaseDecimalCount(0))
        };
    }

    public DbaseField BO_WS_OIDN => this.GetField();
    public DbaseField OK_OIDN => this.GetField();
    public DbaseField ON_WS_OIDN => this.GetField();
    public DbaseField TYPE => this.GetField();
}
