namespace RoadRegistry.Editor.Projections;

using System;
using System.Collections.Generic;
using System.Text;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Schema;
using Schema.Organizations;

public class OrganizationRecordProjection : ConnectedProjection<EditorContext>
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
                }.ToBytes(manager, encoding)
            };

            await context.Organizations.AddAsync(organization, token);
        });

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            var organization = await context.Organizations.SingleAsync(o => o.Code == envelope.Message.Code, token);
            organization.DbaseRecord = new OrganizationDbaseRecord
            {
                ORG = { Value = envelope.Message.Code },
                LBLORG = { Value = envelope.Message.Name }
            }.ToBytes(manager, encoding);
        });
    }

    public static string GetSortableCodeFor(string code)
    {
        return SortableCodeAnomalies.ContainsKey(code)
            ? SortableCodeAnomalies[code]
            : code;
    }
}
