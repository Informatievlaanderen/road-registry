namespace RoadRegistry.BackOffice.Extracts.Dbase.Organizations.V2;

using Be.Vlaanderen.Basisregisters.Shaperon;

public class OrganizationDbaseRecord : DbaseRecord
{
    public static readonly OrganizationDbaseSchema Schema = new();
    public const string DbaseSchemaVersion = WellKnownDbaseSchemaVersions.V2;

    public OrganizationDbaseRecord()
    {
        ORG = new TrimmedDbaseString(Schema.ORG);
        LBLORG = new TrimmedDbaseString(Schema.LBLORG);
        OVOCODE = new TrimmedDbaseString(Schema.OVOCODE);

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
