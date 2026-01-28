namespace RoadRegistry.Extracts.Schemas.Inwinning.Organizations;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class OrganizationDbaseSchema : DbaseSchema
{
    public OrganizationDbaseSchema()
    {
        Fields =
        [
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(ORG)),
                new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLORG)),
                    new DbaseFieldLength(64)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(OVOCODE)),
                    new DbaseFieldLength(9))
        ];
    }

    public DbaseField ORG => Fields[0];
    public DbaseField LBLORG => Fields[1];
    public DbaseField OVOCODE => Fields[2];
}
