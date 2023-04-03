namespace RoadRegistry.BackOffice.Extracts.Dbase.Organizations;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class OrganizationDbaseSchema : DbaseSchema
{
    public OrganizationDbaseSchema()
    {
        Fields = new[]
        {
            DbaseField.CreateCharacterField(
                new DbaseFieldName(nameof(ORG)),
                new DbaseFieldLength(18)),

            DbaseField
                .CreateCharacterField(
                    new DbaseFieldName(nameof(LBLORG)),
                    new DbaseFieldLength(64))
        };
    }

    public DbaseField LBLORG => Fields[1];
    public DbaseField ORG => Fields[0];
}