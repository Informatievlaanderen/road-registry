namespace RoadRegistry.Projections
{
    using Aiv.Vbr.Shaperon;

    public class OrganizationDbaseSchema : DbaseSchema
    {
        public OrganizationDbaseSchema()
        {
            Fields = new DbaseField[]
            {
                DbaseField.CreateStringField(
                    new DbaseFieldName(nameof(ORG)),
                    new DbaseFieldLength(18)),

                DbaseField
                    .CreateStringField(
                        new DbaseFieldName(nameof(LBLORG)),
                        new DbaseFieldLength(64))
            };
        }

        public DbaseField ORG => Fields[0];
        public DbaseField LBLORG => Fields[1];
    }
}
