namespace RoadRegistry.Legacy.Extract.Readers
{
    using System;
    using System.Collections.Generic;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;

    public class ImportedMunicipalitiesReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly ILogger<ImportedMunicipalitiesReader> _logger;

        public ImportedMunicipalitiesReader(IClock clock, ILogger<ImportedMunicipalitiesReader> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return new SqlCommand(
                @"SELECT
                   muni.[naam]
                  ,muni.[NIScode]
                  ,muni.[geometrie]
                FROM [dbo].[gemeenteNIS] muni", connection
            ).YieldEachDataRecord(reader =>
            {
                var name = reader.GetString(0);
                var nisCode = reader.GetString(1);
                _logger.LogDebug("Reading organization with NIS code {0}", nisCode);
                return new StreamEvent(new StreamName("municipality-" + nisCode), new ImportedMunicipality()
                {
                    Geometry = new MunicipalityGeometry()
                    {
                        Polygon = new Ring[0],
                        SpatialReferenceSystemIdentifier = -1,
                    },
                    DutchName = name,
                    NISCode = nisCode,
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                });
            });
        }
    }
}
