namespace RoadRegistry.BackOffice.Model
{
    public static class PredefinedOrganizations
    {
        public static readonly DutchTranslation[] All =
        {
            new DutchTranslation(OrganizationId.Other, "andere"),
            new DutchTranslation(OrganizationId.Unknown, "niet gekend")
        };

        public class DutchTranslation
        {
            internal DutchTranslation(string identifier, string name)
            {
                Identifier = identifier;
                Name = name;
            }

            public string Identifier { get; }
            public string Name { get; }
        }
    }
}
