namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using BackOffice.Messages;
    using BackOffice.Model;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;

    public class ImportedGradeSeparatedJunctionsReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly ILogger<ImportedGradeSeparatedJunctionsReader> _logger;

        public ImportedGradeSeparatedJunctionsReader(IClock clock, ILogger<ImportedGradeSeparatedJunctionsReader> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return new SqlCommand(
                    @"SELECT ok.[ongelijkgrondseKruisingID]
                            ,ok.[bovenWegsegmentID]
                            ,ok.[onderWegsegmentID]
                            ,ok.[type]
                            ,ok.[beginorganisatie]
                            ,lo.[label]
                            ,ok.[begintijd]
                        FROM [dbo].[ongelijkgrondseKruising] ok
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ok.[beginorganisatie] = lo.[code]
                        WHERE ok.[bovenWegsegmentID] IS NOT NULL AND ok.[onderWegsegmentID] IS NOT NULL", connection
                ).YieldEachDataRecord(reader =>
                {
                    var id = reader.GetInt32(0);
                    _logger.LogDebug("Reading grade separated junction with id {0}", id);
                    return new StreamEvent(RoadNetworks.Stream, new ImportedGradeSeparatedJunction
                    {
                        Id = id,
                        UpperRoadSegmentId = reader.GetInt32(1),
                        LowerRoadSegmentId = reader.GetInt32(2),
                        Type = GradeSeparatedJunctionType.ByIdentifier[reader.GetInt32(3)],
                        Origin = new OriginProperties
                        {
                            OrganizationId = reader.GetNullableString(4),
                            Organization = reader.GetNullableString(5),
                            Since = reader.GetDateTime(6)
                        },
                        When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                    });
                });
        }
    }
}
