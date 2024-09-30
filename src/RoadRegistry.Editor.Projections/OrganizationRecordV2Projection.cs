namespace RoadRegistry.Editor.Projections;

using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.Organizations;

//TODO-rik unit tests

public class OrganizationRecordV2Projection : ConnectedProjection<EditorContext>
{
    public OrganizationRecordV2Projection()
    {
        When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
        {
            var organization = new OrganizationRecordV2
            {
                Code = envelope.Message.Code,
                Name = envelope.Message.Name
            };

            await context.OrganizationsV2.AddAsync(organization, token);
        });

        When<Envelope<CreateOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = await context.OrganizationsV2
                .IncludeLocalSingleOrDefaultAsync(o => o.Code == envelope.Message.Code, token);
            if (organization is null)
            {
                organization = new OrganizationRecordV2
                {
                    Code = envelope.Message.Code
                };

                await context.OrganizationsV2.AddAsync(organization, token);
            }

            organization.Name = envelope.Message.Name;
            organization.OvoCode = envelope.Message.OvoCode;
            organization.KboNumber = envelope.Message.KboNumber;
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = await context.OrganizationsV2
                .IncludeLocalSingleAsync(o => o.Code == envelope.Message.Code, token);

            organization.Name = envelope.Message.Name;
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = await context.OrganizationsV2
                .IncludeLocalSingleAsync(o => o.Code == envelope.Message.Code, token);

            organization.Name = envelope.Message.Name;
            organization.OvoCode = envelope.Message.OvoCode;
            organization.KboNumber = envelope.Message.KboNumber;
            organization.IsMaintainer = envelope.Message.IsMaintainer;
        });

        When<Envelope<DeleteOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = await context.OrganizationsV2.IncludeLocalSingleOrDefaultAsync(o => o.Code == envelope.Message.Code, token);

            if (organization is not null)
            {
                context.Remove(organization);
            }
        });
    }
}
