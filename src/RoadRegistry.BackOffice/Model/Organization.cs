namespace RoadRegistry.BackOffice.Model
{
    using System;
    using Framework;
    using Messages;

    public class Organization : EventSourcedEntity
    {
        public static class PredefinedTranslations
        {
            public static readonly DutchTranslation Other = new DutchTranslation(OrganizationId.Other, "andere");
            public static readonly DutchTranslation Unknown = new DutchTranslation(OrganizationId.Unknown, "niet gekend");
            public static readonly DutchTranslation[] All = { Other, Unknown };
        }

        public static readonly Func<Organization> Factory = () => new Organization();

        private DutchTranslation _translation;

        private Organization()
        {
            On<ImportedOrganization>(e => _translation = new DutchTranslation(e.Code, e.Name));
        }

        public DutchTranslation Translation => _translation;

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
