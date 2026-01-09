namespace RoadRegistry.Extracts.Schemas.UploadV2;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Infrastructure.Extensions;

public class RoadSegmentNationalRoadAttributeDbaseSchema : DbaseSchema
{
    public RoadSegmentNationalRoadAttributeDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(NW_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(IDENT2)),
                    new DbaseFieldLength(8)),
        };
    }

    public DbaseField IDENT2 => this.GetField();
    public DbaseField NW_OIDN => this.GetField();
    public DbaseField WS_OIDN => this.GetField();
}
