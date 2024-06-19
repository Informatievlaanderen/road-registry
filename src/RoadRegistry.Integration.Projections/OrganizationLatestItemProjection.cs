namespace RoadRegistry.Integration.Projections;

using System.Collections.Generic;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.Organizations;

public class OrganizationLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    private static readonly Dictionary<string, string> SortableCodeAnomalies =
        new()
        {
            { OrganizationId.Other, "00007" },
            { OrganizationId.Unknown, "00008" }
        };

    public OrganizationLatestItemProjection()
    {
        When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
        {
            var organization = new OrganizationLatestItem
            {
                Code = envelope.Message.Code,
                SortableCode = GetSortableCodeFor(envelope.Message.Code),
                Name = envelope.Message.Name
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
                    SortableCode = GetSortableCodeFor(envelope.Message.Code),
                    Name = envelope.Message.Name,
                    OvoCode = envelope.Message.OvoCode
                };

                await context.Organizations.AddAsync(organization, token);
            }
            else
            {
                organization.Code = envelope.Message.Code;
                organization.SortableCode = GetSortableCodeFor(envelope.Message.Code);
                organization.Name = envelope.Message.Name;
                organization.OvoCode = envelope.Message.OvoCode;
            }
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            organization!.Code = envelope.Message.Code;
            organization.SortableCode = GetSortableCodeFor(envelope.Message.Code);
            organization.Name = envelope.Message.Name;
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            organization!.Code = envelope.Message.Code;
            organization.SortableCode = GetSortableCodeFor(envelope.Message.Code);
            organization.Name = envelope.Message.Name;
            organization.OvoCode = envelope.Message.OvoCode; // What is envelope.Message.OvoCodeModified?
        });

        When<Envelope<DeleteOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization =
                await context.Organizations.FindAsync(envelope.Message.Code, cancellationToken: token);

            if (organization is not null && !organization.IsRemoved)
            {
                organization.IsRemoved = true;
            }
        });
    }

    public static string GetSortableCodeFor(string code)
    {
        return SortableCodeAnomalies.TryGetValue(code, out var anomaly)
            ? anomaly
            : code;
    }
}
