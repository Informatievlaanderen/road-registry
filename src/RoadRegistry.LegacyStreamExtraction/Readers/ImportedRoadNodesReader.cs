namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using BackOffice.Model;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;

    public class ImportedRoadNodesReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly ILogger<ImportedRoadNodesReader> _logger;

        public ImportedRoadNodesReader(IClock clock, WellKnownBinaryReader wkbReader, ILogger<ImportedRoadNodesReader> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<RecordedEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return new SqlCommand(
                @"SELECT
                            wk.[wegknoopID],
                            wk.[wegknoopversie],
                            wk.[type],
                            wk.[geometrie].AsBinaryZM(),
                            wk.[beginorganisatie],
                            lo.[label],
                            wk.[begintijd]
                        FROM [wegknoop] wk
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wk.[beginorganisatie] = lo.[code]", connection
            ).YieldEachDataRecord(reader =>
            {
                var id = reader.GetInt32(0);
                _logger.LogDebug("Reading road node with id {0}", id);
                var geometry = _wkbReader.ReadAs<NetTopologySuite.Geometries.Point>(reader.GetAllBytes(3));
                return new RecordedEvent(RoadNetworks.Stream, new ImportedRoadNode
                {
                    Id = id,
                    Version = reader.GetInt32(1),
                    Type = RoadNodeType.ByIdentifier[reader.GetInt32(2)],
                    Geometry = new RoadNodeGeometry
                    {
                        SpatialReferenceSystemIdentifier =
                            SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                        Point = new Point
                        {
                            X = geometry.X,
                            Y = geometry.Y
                        }
                    },
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
