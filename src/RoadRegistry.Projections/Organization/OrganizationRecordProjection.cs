namespace RoadRegistry.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

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
                SortCode = CalcualteSortCode(@event.Code),
                DbaseRecord = new OrganizationDbaseRecord
                {
                    ORG = { Value = @event.Code },
                    LBLORG = { Value = @event.Name },
                }.ToBytes(_encoding)
            };

            return content.AddAsync(organization, token);
        }

        private static readonly IDictionary<string, string> SortCodeAnomalies =
            new Dictionary<string, string>
            {
                { "-7", "00007" },
                { "-8", "00008" },
            };

        public static string CalcualteSortCode(string code)
        {
            return SortCodeAnomalies.ContainsKey(code)
                ? SortCodeAnomalies[code]
                : code;
        }

    }
}
