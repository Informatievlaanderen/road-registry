namespace RoadRegistry.Integration.Projections.Version;

using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.Organizations.Version;

public class OrganizationVersionProjection : ConnectedProjection<IntegrationContext>
{
    public OrganizationVersionProjection()
    {
        When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
        {
            var organization = new OrganizationVersion
            {
                Position = envelope.Position,
                Code = envelope.Message.Code,
                Name = envelope.Message.Name,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
            };

            await context.OrganizationVersions.AddAsync(organization, token);
        });

        When<Envelope<CreateOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organizationVersion =
                await context.LatestOrganizationVersionPosition(envelope.Message.Code, token);

            if (organizationVersion is null)
            {
                organizationVersion = new OrganizationVersion
                {
                    Position = envelope.Position,
                    Code = envelope.Message.Code,
                    Name = envelope.Message.Name,
                    OvoCode = envelope.Message.OvoCode,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                };

                await context.OrganizationVersions.AddAsync(organizationVersion, token);
            }
            else
            {
                organizationVersion.CloneAndApplyEventInfo(
                    envelope.Position,
                    newVersion =>
                    {
                        newVersion.Code = envelope.Message.Code;
                        newVersion.Name = envelope.Message.Name;
                        newVersion.OvoCode = envelope.Message.OvoCode;
                        newVersion.IsRemoved = false;
                        newVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                    });
            }
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await context.CreateNewOrganizationVersion(
                envelope.Message.Code,
                envelope,
                newVersion =>
                {
                    newVersion.Code = envelope.Message.Code;
                    newVersion.Name = envelope.Message.Name;
                    newVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                },
                token);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await context.CreateNewOrganizationVersion(
                envelope.Message.Code,
                envelope,
                newVersion =>
                {
                    newVersion.Code = envelope.Message.Code;
                    newVersion.Name = envelope.Message.Name;
                    newVersion.OvoCode = envelope.Message.OvoCode;
                    newVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                },
                token);
        });

        When<Envelope<DeleteOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await context.CreateNewOrganizationVersion(
                envelope.Message.Code,
                envelope,
                newVersion =>
                {
                    newVersion.IsRemoved = true;
                    newVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                },
                token);
        });
    }
}
