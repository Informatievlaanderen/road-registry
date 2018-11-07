namespace RoadRegistry.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Messages;

    public class OrganizationRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;

        public OrganizationRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedOrganization>>((content, message, token) => HandleImportedOrganization(content, message.Message, token));
        }

        private Task HandleImportedOrganization(ShapeContext content, ImportedOrganization @event, CancellationToken token)
        {
            var organization = new OrganizationRecord
            {
                Code = @event.Code,
                SortableCode = GetSortableCodeFor(@event.Code),
                DbaseRecord = new OrganizationDbaseRecord
                {
                    ORG = { Value = @event.Code },
                    LBLORG = { Value = @event.Name },
                }.ToBytes(_encoding)
            };

            return content.AddAsync(organization, token);
        }

        private static readonly IDictionary<string, string> SortableCodeAnomalies =
            new Dictionary<string, string>
            {
                { "-7", "00007" },
                { "-8", "00008" },
            };

        public static string GetSortableCodeFor(string code)
        {
            return SortableCodeAnomalies.ContainsKey(code)
                ? SortableCodeAnomalies[code]
                : code;
        }

    }
}
