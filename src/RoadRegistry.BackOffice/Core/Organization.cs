namespace RoadRegistry.BackOffice.Core
{
    using System;
    using Framework;
    using Messages;

    public class Organization : EventSourcedEntity
    {
        public static class PredefinedTranslations
        {
            public static readonly DutchTranslation Other = new DutchTranslation(OrganizationId.Other, new OrganizationName("andere"));
            public static readonly DutchTranslation Unknown = new DutchTranslation(OrganizationId.Unknown, new OrganizationName("niet gekend"));
            public static readonly DutchTranslation[] All = { Other, Unknown };
        }

        public static readonly Func<Organization> Factory = () => new Organization();

        private DutchTranslation _translation;

        private Organization()
        {
            On<ImportedOrganization>(e => _translation = new DutchTranslation(new OrganizationId(e.Code), new OrganizationName(e.Name)));
        }

        public DutchTranslation Translation => _translation;

        public class DutchTranslation
        {
            internal DutchTranslation(OrganizationId identifier, OrganizationName name)
            {
                Identifier = identifier;
                Name = name;
            }

            public OrganizationId Identifier { get; }
            public OrganizationName Name { get; }
        }
    }
}
