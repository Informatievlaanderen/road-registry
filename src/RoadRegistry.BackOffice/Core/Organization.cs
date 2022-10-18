namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class Organization : EventSourcedEntity
{
    private Organization()
    {
        On<ImportedOrganization>(e => Translation = new DutchTranslation(new OrganizationId(e.Code), new OrganizationName(e.Name)));
    }

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

    public static readonly Func<Organization> Factory = () => new Organization();

    public static class PredefinedTranslations
    {
        public static readonly DutchTranslation Other = new(OrganizationId.Other, new OrganizationName("andere"));
        public static readonly DutchTranslation Unknown = new(OrganizationId.Unknown, new OrganizationName("niet gekend"));
        public static readonly DutchTranslation[] All = { Other, Unknown };
    }

    public DutchTranslation Translation { get; private set; }
}
