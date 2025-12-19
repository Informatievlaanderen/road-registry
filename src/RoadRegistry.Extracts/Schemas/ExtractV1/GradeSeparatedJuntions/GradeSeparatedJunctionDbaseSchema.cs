namespace RoadRegistry.Extracts.Schemas.ExtractV1.GradeSeparatedJuntions;

using Be.Vlaanderen.Basisregisters.Shaperon;

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
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLTYPE)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(BO_WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),
            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(ON_WS_OIDN)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

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

    public DbaseField BEGINORG => Fields[6];
    public DbaseField BEGINTIJD => Fields[5];
    public DbaseField BO_WS_OIDN => Fields[3];
    public DbaseField LBLBGNORG => Fields[7];
    public DbaseField LBLTYPE => Fields[2];
    public DbaseField OK_OIDN => Fields[0];
    public DbaseField ON_WS_OIDN => Fields[4];
    public DbaseField TYPE => Fields[1];
}
