namespace RoadRegistry.Product.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.Organizations.V2;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using RoadRegistry.BackOffice;
using Schema;

public class OrganizationRecordProjection : ConnectedProjection<ProductContext>
{
    private static readonly IDictionary<string, string> SortableCodeAnomalies =
        new Dictionary<string, string>
        {
            { "-7", "00007" },
            { "-8", "00008" }
        };

    public OrganizationRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);

        When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
        {
            var organization = new OrganizationRecord
            {
                Code = envelope.Message.Code,
                SortableCode = GetSortableCodeFor(envelope.Message.Code),
                DbaseRecord = new OrganizationDbaseRecord
                {
                    ORG = { Value = envelope.Message.Code },
                    LBLORG = { Value = envelope.Message.Name }
                }.ToBytes(manager, encoding),
                DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion
            };

            await context.Organizations.AddAsync(organization, token);
        });

        When<Envelope<CreateOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = context.Organizations.Local.SingleOrDefault(o => o.Code == envelope.Message.Code)
                               ?? await context.Organizations.SingleOrDefaultAsync(o => o.Code == envelope.Message.Code, token);

            if (organization is null)
            {
                organization = new OrganizationRecord
                {
                    Code = envelope.Message.Code,
                    SortableCode = GetSortableCodeFor(envelope.Message.Code),
                    DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion
                };

                await context.Organizations.AddAsync(organization, token);
            }

            organization.DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = envelope.Message.Code },
                LBLORG = { Value = OrganizationName.WithoutExcessLength(envelope.Message.Name) },
                OVOCODE = { Value = envelope.Message.OvoCode }
            }.ToBytes(manager, encoding);
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = context.Organizations.Local.SingleOrDefault(o => o.Code == envelope.Message.Code)
                               ?? await context.Organizations.SingleAsync(o => o.Code == envelope.Message.Code, token);

            organization.DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = envelope.Message.Code },
                LBLORG = { Value = OrganizationName.WithoutExcessLength(envelope.Message.Name) }
            }.ToBytes(manager, encoding);
            organization.DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion;
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = context.Organizations.Local.SingleOrDefault(o => o.Code == envelope.Message.Code)
                               ?? await context.Organizations.SingleAsync(o => o.Code == envelope.Message.Code, token);

            organization.DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = envelope.Message.Code },
                LBLORG = { Value = OrganizationName.WithoutExcessLength(envelope.Message.Name) },
                OVOCODE = { Value = envelope.Message.OvoCode }
            }.ToBytes(manager, encoding);
            organization.DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion;
        });

        When<Envelope<DeleteOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = context.Organizations.Local.SingleOrDefault(o => o.Code == envelope.Message.Code)
                               ?? await context.Organizations.SingleOrDefaultAsync(o => o.Code == envelope.Message.Code, token);
            if (organization is not null)
            {
                context.Remove(organization);
            }
        });
    }

    public static string GetSortableCodeFor(string code)
    {
        return SortableCodeAnomalies.ContainsKey(code)
            ? SortableCodeAnomalies[code]
            : code;
    }
}
