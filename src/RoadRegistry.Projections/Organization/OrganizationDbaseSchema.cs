namespace RoadRegistry.Projections
{
    using Shaperon;

    public class OrganizationDbaseSchema
    {
        public OrganizationDbaseSchema()
        {
            ORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(ORG)),
                new DbaseFieldLength(18));

            LBLORG = DbaseField.CreateStringField(
                new DbaseFieldName(nameof(LBLORG)),
                new DbaseFieldLength(64));
        }

        public DbaseField ORG { get; }
        public DbaseField LBLORG { get; }
    }
}
