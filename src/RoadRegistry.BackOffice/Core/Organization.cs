namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;

public class Organization : EventSourcedEntity
{
    public static readonly Func<Organization> Factory = () => new Organization();

    private Organization()
    {
        On<ImportedOrganization>(e => Translation = new OrganizationName.DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name)));
        On<CreateOrganizationAccepted>(e =>
        {
            Translation = new OrganizationName.DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name));
            OvoCode = OrganizationOvoCode.FromValue(e.OvoCode);
            KboNumber = OrganizationKboNumber.FromValue(e.KboNumber);
        });
        On<RenameOrganizationAccepted>(e => Translation = new OrganizationName.DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name)));
        On<ChangeOrganizationAccepted>(e =>
        {
            Translation = new OrganizationName.DutchTranslation(new OrganizationId(e.Code), OrganizationName.WithoutExcessLength(e.Name));
            OvoCode = OrganizationOvoCode.FromValue(e.OvoCode);
            KboNumber = OrganizationKboNumber.FromValue(e.KboNumber);
            IsMaintainer = e.IsMaintainer;
        });
        On<DeleteOrganizationAccepted>(e =>
        {
            IsRemoved = true;
        });
    }

    public OrganizationName.DutchTranslation Translation { get; private set; }
    public OrganizationOvoCode? OvoCode { get; private set; }
    public OrganizationKboNumber? KboNumber { get; private set; }
    public bool IsMaintainer { get; private set; }

    public bool IsRemoved { get; private set; }

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

    public static OrganizationName.DutchTranslation ToDutchTranslation(Organization organization, OrganizationId organizationId)
    {
        if (organization is null)
        {
            return OrganizationName.PredefinedTranslations.FromSystemValue(organizationId);
        }

        // Can only be enabled once the OVO-code is leading instead of classic OrganizationId.
        // if (organization.OvoCode is not null)
        // {
        //     return new DutchTranslation(new OrganizationId(organization.OvoCode.Value), organization.Translation.Name);
        // }

        return organization.Translation
               ?? ToDutchTranslation(null, organizationId);
    }
}
