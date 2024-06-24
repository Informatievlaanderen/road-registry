namespace RoadRegistry.Integration.Projections;

using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.Organizations;

public class OrganizationLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public OrganizationLatestItemProjection()
    {
        When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
        {
            var organization = new OrganizationLatestItem
            {
                Code = envelope.Message.Code,
                Name = envelope.Message.Name,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
            };

            await context.Organizations.AddAsync(organization, token);
        });

        When<Envelope<CreateOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            if (organization is null)
            {
                organization = new OrganizationLatestItem
                {
                    Code = envelope.Message.Code,
                    Name = envelope.Message.Name,
                    OvoCode = envelope.Message.OvoCode,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };

                await context.Organizations.AddAsync(organization, token);
            }
            else
            {
                organization.Code = envelope.Message.Code;
                organization.Name = envelope.Message.Name;
                organization.OvoCode = envelope.Message.OvoCode;
                organization.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                organization.IsRemoved = false;
            }
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            organization!.Code = envelope.Message.Code;
            organization.Name = envelope.Message.Name;
            organization.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            organization!.Code = envelope.Message.Code;
            organization.Name = envelope.Message.Name;
            organization.OvoCode = envelope.Message.OvoCode;
            organization.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        });

        When<Envelope<DeleteOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            if (organization is not null && !organization.IsRemoved)
            {
                organization.IsRemoved = true;
                organization.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            }
        });
    }
}
