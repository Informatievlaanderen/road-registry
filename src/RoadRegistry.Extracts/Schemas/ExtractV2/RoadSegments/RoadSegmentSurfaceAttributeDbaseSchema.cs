namespace RoadRegistry.Extracts.Schemas.ExtractV2.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;

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
                .CreateCharacterField(
                    new DbaseFieldName(nameof(WS_GIDN)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(VANPOS)),
                    new DbaseFieldLength(9),
                    new DbaseDecimalCount(3)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TOTPOS)),
                    new DbaseFieldLength(9),
                    new DbaseDecimalCount(3)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINTIJD)),
                    new DbaseFieldLength(15)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(BEGINORG)),
                    new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLBGNORG)),
                    new DbaseFieldLength(64))
        };
    }

    public DbaseField BEGINORG => Fields[8];
    public DbaseField BEGINTIJD => Fields[7];
    public DbaseField LBLBGNORG => Fields[9];
    public DbaseField LBLTYPE => Fields[4];
    public DbaseField TOTPOS => Fields[6];
    public DbaseField TYPE => Fields[3];
    public DbaseField VANPOS => Fields[5];
    public DbaseField WS_GIDN => Fields[2];
    public DbaseField WS_OIDN => Fields[1];
    public DbaseField WV_OIDN => Fields[0];
}
