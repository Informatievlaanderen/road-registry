namespace RoadRegistry.BackOffice.Schema.Organizations
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class OrganizationDbaseRecord : DbaseRecord
    {
        public static readonly OrganizationDbaseSchema Schema = new OrganizationDbaseSchema();

        public OrganizationDbaseRecord()
        {
            ORG = new DbaseCharacter(Schema.ORG);
            LBLORG = new DbaseCharacter(Schema.LBLORG);

            Values = new DbaseFieldValue[]
            {
                ORG,
                LBLORG,
            };
        }

        public DbaseCharacter ORG { get; }
        public DbaseCharacter LBLORG { get; }
    }
}
