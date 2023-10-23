namespace RoadRegistry.Product.Projections;

using BackOffice;
using BackOffice.Abstractions.Organizations;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.Organizations.V2;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class OrganizationRecordProjection : ConnectedProjection<ProductContext>
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly Encoding _encoding;

    private static readonly IDictionary<string, string> SortableCodeAnomalies =
        new Dictionary<string, string>
        {
            { OrganizationId.Other, "00007" },
            { OrganizationId.Unknown, "00008" }
        };

    public OrganizationRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        _manager = manager.ThrowIfNull();
        _encoding = encoding.ThrowIfNull();

        When<Envelope<ImportedOrganization>>(async (context, envelope, token) =>
        {
            var organization = new OrganizationRecord
            {
                Code = envelope.Message.Code,
                SortableCode = GetSortableCodeFor(envelope.Message.Code)
            };
            Update(organization, envelope.Message.Code, envelope.Message.Name, null);

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

            Update(organization, envelope.Message.Code, envelope.Message.Name, envelope.Message.OvoCode);
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = context.Organizations.Local.SingleOrDefault(o => o.Code == envelope.Message.Code)
                               ?? await context.Organizations.SingleAsync(o => o.Code == envelope.Message.Code, token);

            var organizationDetail = new OrganizationDbaseRecordReader(manager, encoding)
                .Read(organization.DbaseRecord, organization.DbaseSchemaVersion);

            Update(organization, envelope.Message.Code, envelope.Message.Name, organizationDetail.OvoCode);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = context.Organizations.Local.SingleOrDefault(o => o.Code == envelope.Message.Code)
                               ?? await context.Organizations.SingleAsync(o => o.Code == envelope.Message.Code, token);

            Update(organization, envelope.Message.Code, envelope.Message.Name, envelope.Message.OvoCode);
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
        return SortableCodeAnomalies.TryGetValue(code, out var anomaly)
            ? anomaly
            : code;
    }

    private void Update(OrganizationRecord organization, string code, string name, string ovoCode)
    {
        var dbaseRecord = new OrganizationDbaseRecord
        {
            ORG = { Value = new OrganizationId(code) },
            LBLORG = { Value = OrganizationName.WithoutExcessLength(name) }
        };
        if (ovoCode is not null)
        {
            dbaseRecord.OVOCODE.Value = new OrganizationOvoCode(ovoCode);
        }

        organization.DbaseRecord = dbaseRecord.ToBytes(_manager, _encoding);
        organization.DbaseSchemaVersion = OrganizationDbaseRecord.DbaseSchemaVersion;
    }
}
