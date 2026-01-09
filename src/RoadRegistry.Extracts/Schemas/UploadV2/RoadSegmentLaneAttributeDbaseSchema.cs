// ReSharper disable InconsistentNaming

namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

public class RoadSegmentLaneAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentLaneAttributeDbaseSchema()
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
                    new DbaseFieldName(nameof(AANTAL)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(RICHTING)),
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

    public DbaseField AANTAL => this.GetField();
    public DbaseField RICHTING => this.GetField();
    public DbaseField RS_OIDN => this.GetField();
    public DbaseField TOTPOS => this.GetField();
    public DbaseField VANPOS => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
}
