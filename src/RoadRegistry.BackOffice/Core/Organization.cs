namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class Organization : EventSourcedEntity
{
    public static readonly Func<Organization> Factory = () => new Organization();

    private Organization()
    {
        On<ImportedOrganization>(e => Translation = new DutchTranslation(new OrganizationId(e.Code), new OrganizationName(e.Name)));
    }

    public DutchTranslation Translation { get; private set; }

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

    public static class PredefinedTranslations
    {
        public static readonly DutchTranslation Other = new(OrganizationId.Other, new OrganizationName("andere"));
        public static readonly DutchTranslation Unknown = new(OrganizationId.Unknown, new OrganizationName("niet gekend"));
        public static readonly DutchTranslation[] All = { Other, Unknown };
    }
}