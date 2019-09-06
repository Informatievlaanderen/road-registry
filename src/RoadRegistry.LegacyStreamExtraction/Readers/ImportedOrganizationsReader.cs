namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;

    public class ImportedOrganizationsReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly ILogger<ImportedOrganizationsReader> _logger;

        public ImportedOrganizationsReader(IClock clock, ILogger<ImportedOrganizationsReader> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyCollection<RecordedEvent>> ReadAsync(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var events = new List<RecordedEvent>();

            await new SqlCommand(
                @"SELECT
                            org.[Code],
                            org.[Label]
                        FROM [listOrganisatie] org", connection
            ).ForEachDataRecord(reader =>
            {
                var code = reader.GetString(0);
                _logger.LogDebug("Reading organization with code {0}", code);
                events.Add(new RecordedEvent(new StreamName("organization-" + code), new ImportedOrganization
                {
                    Code = code,
                    Name = reader.GetString(1),
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }));
            });

            return events;
        }
    }
}
