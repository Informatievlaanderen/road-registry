namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.IO;
    using Schema;
    using Schema.Organizations;

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
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedOrganization>>(async (content, envelope, token) =>
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

                await content.AddAsync(organization, token);
            });
        }

        public static string GetSortableCodeFor(string code)
        {
            return SortableCodeAnomalies.ContainsKey(code)
                ? SortableCodeAnomalies[code]
                : code;
        }
    }
}
