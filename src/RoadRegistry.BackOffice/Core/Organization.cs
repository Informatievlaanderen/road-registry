namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class Organization : EventSourcedEntity
{
    public static readonly Func<Organization> Factory = () => new Organization();

    private Organization()
    {
        On<ImportedOrganization>(e => Translation = new DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name)));
        On<CreateOrganizationAccepted>(e =>
        {
            Translation = new DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name));
            OvoCode = OrganizationOvoCode.FromValue(e.OvoCode);
            KboNumber = OrganizationKboNumber.FromValue(e.KboNumber);
        });
        On<RenameOrganizationAccepted>(e => Translation = new DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name)));
        On<ChangeOrganizationAccepted>(e =>
        {
            Translation = new DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name));
            OvoCode = OrganizationOvoCode.FromValue(e.OvoCode);
            KboNumber = OrganizationKboNumber.FromValue(e.KboNumber);
            IsMaintainer = e.IsMaintainer;
        });
        On<DeleteOrganizationAccepted>(e =>
        {
            IsRemoved = true;
        });
    }

    public DutchTranslation Translation { get; private set; }
    public OrganizationOvoCode? OvoCode { get; private set; }
    public OrganizationKboNumber? KboNumber { get; private set; }
    public bool IsMaintainer { get; private set; }

    public bool IsRemoved { get; private set; }

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
        public static readonly DutchTranslation[] All = [Other, Unknown];

        public static DutchTranslation FromSystemValue(OrganizationId organizationId)
        {
            return organizationId == OrganizationId.Other
                ? Other
                : Unknown;
        }
    }

    public void Rename(OrganizationName name)
    {
        Apply(new RenameOrganizationAccepted
        {
            Code = Translation.Identifier,
            Name = name
        });
    }

    public void Change(
        OrganizationName? name,
        OrganizationOvoCode? ovoCode,
        OrganizationKboNumber? kboNumber,
        bool? isMaintainer)
    {
        Apply(new ChangeOrganizationAccepted
        {
            Code = Translation.Identifier,
            Name = name ?? Translation.Name,
            NameModified = name is not null && name != Translation.Name,
            OvoCode = ovoCode ?? OvoCode,
            OvoCodeModified = ovoCode is not null && ovoCode != OvoCode,
            KboNumber = kboNumber ?? KboNumber,
            KboNumberModified = kboNumber is not null && kboNumber != KboNumber,
            IsMaintainer = isMaintainer ?? IsMaintainer,
            IsMaintainerModified = isMaintainer is not null && isMaintainer != IsMaintainer,
        });
    }

    public void Delete()
    {
        Apply(new DeleteOrganizationAccepted
        {
            Code = Translation.Identifier
        });
    }

    public static DutchTranslation ToDutchTranslation(Organization organization, OrganizationId organizationId)
    {
        if (organization is null)
        {
            return PredefinedTranslations.FromSystemValue(organizationId);
        }

        if (organization.OvoCode is not null)
        {
            return new DutchTranslation(new OrganizationId(organization.OvoCode.Value), organization.Translation.Name);
        }

        return organization.Translation
               ?? ToDutchTranslation(null, organizationId);
    }
}
