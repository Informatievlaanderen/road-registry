namespace RoadRegistry.Integration.Projections;

using System;
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
                    KboNumber = envelope.Message.KboNumber,
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
                organization.KboNumber = envelope.Message.KboNumber;
                organization.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                organization.IsRemoved = false;
            }
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            // Unlike the change event below, a rename carries nothing but the code and the new name, so there is no
            // organization to build out of it. Reported as what it is - this read model is being told to rename an
            // organization it does not have - rather than as a NullReferenceException that names neither the
            // organization nor what was being done to it.
            if (organization is null)
            {
                throw new InvalidOperationException($"{nameof(OrganizationLatestItem)} with code {envelope.Message.Code} is not found");
            }

            organization.Name = envelope.Message.Name;
            organization.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        });

        // The event carries the organization in full, so a row that is not there yet is built from it instead of
        // dereferenced - the same upsert the create handler above does. Without it, a change for an organization this
        // read model never saw created took the message pump down with a NullReferenceException, which is what
        // happened on stg.
        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            if (organization is null)
            {
                organization = new OrganizationLatestItem
                {
                    Code = envelope.Message.Code,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };

                await context.Organizations.AddAsync(organization, token);
            }

            organization.Name = envelope.Message.Name;
            organization.OvoCode = envelope.Message.OvoCode;
            organization.KboNumber = envelope.Message.KboNumber;
            organization.IsMaintainer = envelope.Message.IsMaintainer;
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
