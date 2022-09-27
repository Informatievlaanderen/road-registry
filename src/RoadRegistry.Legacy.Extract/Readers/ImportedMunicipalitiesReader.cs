namespace RoadRegistry.Legacy.Extract.Readers
{
    using System;
    using System.Collections.Generic;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using NodaTime.Text;
    using Point = BackOffice.Messages.Point;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    public class ImportedMunicipalitiesReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly ILogger<ImportedMunicipalitiesReader> _logger;
        private readonly WellKnownBinaryReader _wkbReader;

        public ImportedMunicipalitiesReader(IClock clock,
            WellKnownBinaryReader wkbReader,
            ILogger<ImportedMunicipalitiesReader> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _wkbReader = wkbReader;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return new SqlCommand(
                @"SELECT
                   muni.[naam]
                  ,muni.[NIScode]
                  ,muni.[geometrie].AsBinaryZM()
                FROM [dbo].[gemeenteNIS] muni", connection
            ).YieldEachDataRecord(reader =>
            {
                var name = reader.GetString(0);
                var nisCode = reader.GetString(1);
                var wellKnownBinary = reader.GetAllBytes(2);
                var multiPolygon = _wkbReader
                    .TryReadAs(wellKnownBinary, out Polygon polygon)
                    ? new MultiPolygon(new[] { polygon })
                    : _wkbReader.ReadAs<MultiPolygon>(wellKnownBinary);

                _logger.LogDebug("Reading organization with NIS code {0}", nisCode);
                return new StreamEvent(new StreamName("municipality-" + nisCode), new ImportedMunicipality
                {
                    Geometry = new MunicipalityGeometry
                    {
                        MultiPolygon = Array.ConvertAll(multiPolygon.Geometries, geometry =>
                        {
                            var polygonGeometry = (Polygon)geometry;
                            return new BackOffice.Messages.Polygon
                            {
                                Shell = new Ring
                                {
                                    Points = Array.ConvertAll(polygonGeometry.ExteriorRing.Coordinates,
                                        coordinate =>
                                            new Point
                                            {
                                                X = coordinate.X,
                                                Y = coordinate.Y
                                            })
                                },
                                Holes = Array.ConvertAll(polygonGeometry.Holes, hole =>
                                    new Ring
                                    {
                                        Points = Array.ConvertAll(hole.Coordinates, coordinate =>
                                            new Point
                                            {
                                                X = coordinate.X,
                                                Y = coordinate.Y
                                            })
                                    })
                            };
                        }),
                        SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32()
                    },
                    DutchName = name,
                    NISCode = nisCode,
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                });
            });
        }
    }
}
