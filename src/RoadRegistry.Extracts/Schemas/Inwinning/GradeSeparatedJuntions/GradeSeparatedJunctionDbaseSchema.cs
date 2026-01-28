namespace RoadRegistry.Extracts.Schemas.Inwinning.GradeSeparatedJuntions;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class GradeSeparatedJunctionDbaseSchema : DbaseSchema
{
    public GradeSeparatedJunctionDbaseSchema()
    {
        Fields =
        [
            DbaseField.CreateNumberField(
                new DbaseFieldName(nameof(OK_OIDN)),
                new DbaseFieldLength(15),
                new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(BO_TEMPID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),
            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(ON_TEMPID)),
                    new DbaseFieldLength(15),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateNumberField(
                    new DbaseFieldName(nameof(TYPE)),
                    new DbaseFieldLength(2),
                    new DbaseDecimalCount(0)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(CREATIE)),
                    new DbaseFieldLength(15))
        ];
    }

    public DbaseField OK_OIDN => Fields[0];
    public DbaseField BO_TEMPID => Fields[1];
    public DbaseField ON_TEMPID => Fields[2];
    public DbaseField TYPE => Fields[3];
    public DbaseField CREATIE => Fields[4];
}
