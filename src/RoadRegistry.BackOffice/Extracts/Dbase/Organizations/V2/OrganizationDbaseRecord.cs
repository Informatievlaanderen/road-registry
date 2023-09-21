namespace RoadRegistry.BackOffice.Extracts.Dbase.Organizations.V2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class OrganizationDbaseRecord : DbaseRecord
{
    public static readonly OrganizationDbaseSchema Schema = new();
    public const string DbaseSchemaVersion = WellKnownDbaseSchemaVersions.V2;

    public OrganizationDbaseRecord()
    {
        ORG = new DbaseString(Schema.ORG);
        LBLORG = new DbaseString(Schema.LBLORG);
        OVOCODE = new DbaseString(Schema.OVOCODE);

        Values = new DbaseFieldValue[]
        {
            ORG,
            LBLORG,
            OVOCODE
        };
    }

    public DbaseString LBLORG { get; }
    public DbaseString ORG { get; }
    public DbaseString OVOCODE { get; }
}
