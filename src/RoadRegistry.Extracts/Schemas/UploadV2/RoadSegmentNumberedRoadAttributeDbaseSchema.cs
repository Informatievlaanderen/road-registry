namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

public class RoadSegmentNumberedRoadAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentNumberedRoadAttributeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(GW_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15),
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
                    new DbaseDecimalCount(0))
        };
    }

    public DbaseField GW_OIDN => this.GetField();
    public DbaseField IDENT8 => this.GetField();
    public DbaseField RICHTING => this.GetField();
    public DbaseField VOLGNUMMER => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
}
