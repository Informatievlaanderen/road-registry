namespace RoadRegistry.Legacy.Extract.Readers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;

    public class ImportedRoadSegmentsReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly WellKnownBinaryReader _reader;
        private readonly ILogger<ImportedRoadSegmentsReader> _logger;

        public ImportedRoadSegmentsReader(IClock clock, WellKnownBinaryReader reader, ILogger<ImportedRoadSegmentsReader> logger)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private class RoadSegmentEnumerator : IEnumerable<ImportedRoadSegment>, IEnumerator<ImportedRoadSegment>
        {
            private readonly SqlConnection _connection;
            private readonly IClock _clock;
            private readonly WellKnownBinaryReader _reader;
            private readonly ILogger<ImportedRoadSegmentsReader> _logger;
            private State _state;
            private ImportedRoadSegment[] _batch;
            private int _index;

            private enum State { Initial, Read, Final }

            public RoadSegmentEnumerator(SqlConnection connection, IClock clock, WellKnownBinaryReader reader, ILogger<ImportedRoadSegmentsReader> logger)
            {
                _connection = connection;
                _clock = clock;
                _reader = reader;
                _logger = logger;

                _state = State.Initial;
                _batch = Array.Empty<ImportedRoadSegment>();
                _index = -1;
            }

            private ImportedRoadSegment[] ReadInitialBatch()
            {
                var events = new List<ImportedRoadSegment>(1000);
                new SqlCommand(
                        @"SELECT TOP 1000
                        ws.[wegsegmentID], --0
                        ws.[wegsegmentversie], --1
                        ws.[beginWegknoopID], --2
                        ws.[eindWegknoopID], --3
                        ws.[geometrie].AsBinaryZM(), --4
                        ws.[geometrieversie], --5
                        ws.[beheerder], --6
                        beheerders.[label], --7
                        ws.[methode], --8
                        ws.[morfologie], --9
                        ws.[status], --10
                        ws.[categorie], --11
                        ws.[toegangsbeperking], --12
                        ws.[linksStraatnaamID], --13
                        ISNULL(ls.[LOS], ''), --14 --streetname is empty string when not found
                        lg.[NISCode], --15
                        lg.[naam], --16
                        ws.[rechtsStraatnaamID], --17
                        ISNULL(rs.[LOS], ''), --18 -streetname is empty string when not found
                        rg.[NISCode], --19
                        rg.[naam], --20
                        ws.[opnamedatum], --21
                        ws.[beginorganisatie], --22
                        lo.[label], --23
                        ws.[beginoperator], --24
                        ws.[beginapplicatie], --25
                        ws.[begintijd],--26
                        ws.[transactieID] --27
                    FROM [dbo].[wegsegment] ws
                    LEFT OUTER JOIN [dbo].[gemeenteNIS] lg ON ws.[linksGemeente] = lg.[gemeenteId]
                    LEFT OUTER JOIN [dbo].[crabsnm] ls ON ws.[linksStraatnaamID] = ls.[EXN]
                    LEFT OUTER JOIN [dbo].[gemeenteNIS] rg ON ws.[rechtsGemeente] = rg.[gemeenteId]
                    LEFT OUTER JOIN [dbo].[crabsnm] rs ON ws.[rechtsStraatnaamID] = rs.[EXN]
                    LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ws.[beginorganisatie] = lo.[code]
                    LEFT OUTER JOIN [dbo].[listOrganisatie] beheerders ON ws.[beheerder] = beheerders.[code]
                    WHERE ws.[eindWegknoopID] IS NOT NULL AND ws.[beginWegknoopID] IS NOT NULL
                    ORDER BY ws.[wegsegmentID]",
                        _connection
                    )
                    .ForEachDataRecord(reader =>
                    {
                        var id = reader.GetInt32(0);
                        _logger.LogDebug("Reading road segment with id {0}", id);
                        var wellKnownBinary = reader.GetAllBytes(4);
                        var geometry = _reader
                            .TryReadAs(wellKnownBinary, out NetTopologySuite.Geometries.LineString oneLine)
                            ? new NetTopologySuite.Geometries.MultiLineString(new[] {oneLine})
                            : _reader.ReadAs<NetTopologySuite.Geometries.MultiLineString>(wellKnownBinary);

                        var multiLineString = Array.ConvertAll(
                            geometry.Geometries.Cast<NetTopologySuite.Geometries.LineString>().ToArray(),
                            input => new BackOffice.Messages.LineString
                            {
                                Points = Array.ConvertAll(
                                    input.Coordinates,
                                    coordinate => new BackOffice.Messages.Point
                                    {
                                        X = coordinate.X,
                                        Y = coordinate.Y
                                    }),
                                Measures = geometry.GetOrdinates(NetTopologySuite.Geometries.Ordinate.M)
                            });

                        events.Add(new ImportedRoadSegment
                        {
                            Id = id,
                            Version = reader.GetInt32(1),
                            StartNodeId = reader.GetInt32(2),
                            EndNodeId = reader.GetInt32(3),
                            Geometry = new RoadSegmentGeometry
                            {
                                SpatialReferenceSystemIdentifier =
                                    SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                MultiLineString = multiLineString
                            },
                            GeometryVersion = reader.GetInt32(5),
                            MaintenanceAuthority = new MaintenanceAuthority
                            {
                                Code = reader.GetString(6),
                                Name = reader.GetString(7)
                            },
                            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[reader.GetInt32(8)],
                            Morphology = RoadSegmentMorphology.ByIdentifier[reader.GetInt32(9)],
                            Status = RoadSegmentStatus.ByIdentifier[reader.GetInt32(10)],
                            Category = RoadSegmentCategory.ByIdentifier[reader.GetString(11)],
                            AccessRestriction = RoadSegmentAccessRestriction.ByIdentifier[reader.GetInt32(12)],
                            LeftSide = new ImportedRoadSegmentSideAttributes
                            {
                                StreetNameId = reader.GetNullableInt32(13),
                                StreetName = reader.GetNullableString(14),
                                MunicipalityNISCode = reader.GetNullableString(15),
                                Municipality = reader.GetNullableString(16)
                            },
                            RightSide = new ImportedRoadSegmentSideAttributes
                            {
                                StreetNameId = reader.GetNullableInt32(17),
                                StreetName = reader.GetNullableString(18),
                                MunicipalityNISCode = reader.GetNullableString(19),
                                Municipality = reader.GetNullableString(20)
                            },
                            RecordingDate = reader.GetDateTime(21),
                            Origin = new ImportedOriginProperties
                            {
                                OrganizationId = reader.GetNullableString(22),
                                Organization = reader.GetNullableString(23),
                                Operator = reader.GetNullableString(24),
                                Application = reader.GetNullableString(25),
                                Since = reader.GetDateTime(26),
                                TransactionId = reader.GetNullableInt32(27) ?? TransactionId.Unknown.ToInt32()
                            },
                            PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttributes>(),
                            PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttributes>(),
                            PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttributes>(),
                            Lanes = Array.Empty<ImportedRoadSegmentLaneAttributes>(),
                            Widths = Array.Empty<ImportedRoadSegmentWidthAttributes>(),
                            Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttributes>(),
                            When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                        });
                    });
                return events.ToArray();
            }

            private ImportedRoadSegment[] ReadNextBatch(int afterSegmentId)
            {
                var events = new List<ImportedRoadSegment>(1000);
                new SqlCommand(
                        @"SELECT TOP 1000
                        ws.[wegsegmentID], --0
                        ws.[wegsegmentversie], --1
                        ws.[beginWegknoopID], --2
                        ws.[eindWegknoopID], --3
                        ws.[geometrie].AsBinaryZM(), --4
                        ws.[geometrieversie], --5
                        ws.[beheerder], --6
                        beheerders.[label], --7
                        ws.[methode], --8
                        ws.[morfologie], --9
                        ws.[status], --10
                        ws.[categorie], --11
                        ws.[toegangsbeperking], --12
                        ws.[linksStraatnaamID], --13
                        ISNULL(ls.[LOS], ''), --14 --streetname is empty string when not found
                        lg.[NISCode], --15
                        lg.[naam], --16
                        ws.[rechtsStraatnaamID], --17
                        ISNULL(rs.[LOS], ''), --18 -streetname is empty string when not found
                        rg.[NISCode], --19
                        rg.[naam], --20
                        ws.[opnamedatum], --21
                        ws.[beginorganisatie], --22
                        lo.[label], --23
                        ws.[beginoperator], --24
                        ws.[beginapplicatie], --25
                        ws.[begintijd],--26
                        ws.[transactieID] --27
                    FROM [dbo].[wegsegment] ws
                    LEFT OUTER JOIN [dbo].[gemeenteNIS] lg ON ws.[linksGemeente] = lg.[gemeenteId]
                    LEFT OUTER JOIN [dbo].[crabsnm] ls ON ws.[linksStraatnaamID] = ls.[EXN]
                    LEFT OUTER JOIN [dbo].[gemeenteNIS] rg ON ws.[rechtsGemeente] = rg.[gemeenteId]
                    LEFT OUTER JOIN [dbo].[crabsnm] rs ON ws.[rechtsStraatnaamID] = rs.[EXN]
                    LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ws.[beginorganisatie] = lo.[code]
                    LEFT OUTER JOIN [dbo].[listOrganisatie] beheerders ON ws.[beheerder] = beheerders.[code]
                    WHERE ws.[eindWegknoopID] IS NOT NULL AND ws.[beginWegknoopID] IS NOT NULL
                    AND ws.[wegsegmentID] > @P0
                    ORDER BY ws.[wegsegmentID]",
                        _connection
                    )
                    {
                        Parameters = { new SqlParameter("@P0", SqlDbType.Int, 4, ParameterDirection.Input, false, 0,0, "", DataRowVersion.Default, afterSegmentId)}
                    }
                    .ForEachDataRecord(reader =>
                    {
                        var id = reader.GetInt32(0);
                        _logger.LogDebug("Reading road segment with id {0}", id);
                        var wellKnownBinary = reader.GetAllBytes(4);
                        var geometry = _reader
                            .TryReadAs(wellKnownBinary, out NetTopologySuite.Geometries.LineString oneLine)
                            ? new NetTopologySuite.Geometries.MultiLineString(new [] {oneLine})
                            : _reader.ReadAs<NetTopologySuite.Geometries.MultiLineString>(wellKnownBinary);

                        var multiLineString = Array.ConvertAll(
                            geometry.Geometries.Cast<NetTopologySuite.Geometries.LineString>().ToArray(),
                            input => new LineString
                            {
                                Points = Array.ConvertAll(
                                    input.Coordinates,
                                    coordinate => new BackOffice.Messages.Point
                                    {
                                        X = coordinate.X,
                                        Y = coordinate.Y
                                    }),
                                Measures = geometry.GetOrdinates(NetTopologySuite.Geometries.Ordinate.M)
                            });

                        events.Add(new ImportedRoadSegment
                        {
                            Id = id,
                            Version = reader.GetInt32(1),
                            StartNodeId = reader.GetInt32(2),
                            EndNodeId = reader.GetInt32(3),
                            Geometry = new RoadSegmentGeometry
                            {
                                SpatialReferenceSystemIdentifier =
                                    SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                MultiLineString = multiLineString
                            },
                            GeometryVersion = reader.GetInt32(5),
                            MaintenanceAuthority = new MaintenanceAuthority
                            {
                                Code = reader.GetString(6),
                                Name = reader.GetString(7)
                            },
                            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[reader.GetInt32(8)],
                            Morphology = RoadSegmentMorphology.ByIdentifier[reader.GetInt32(9)],
                            Status = RoadSegmentStatus.ByIdentifier[reader.GetInt32(10)],
                            Category = RoadSegmentCategory.ByIdentifier[reader.GetString(11)],
                            AccessRestriction = RoadSegmentAccessRestriction.ByIdentifier[reader.GetInt32(12)],
                            LeftSide = new ImportedRoadSegmentSideAttributes
                            {
                                StreetNameId = reader.GetNullableInt32(13),
                                StreetName = reader.GetNullableString(14),
                                MunicipalityNISCode = reader.GetNullableString(15),
                                Municipality = reader.GetNullableString(16)
                            },
                            RightSide = new ImportedRoadSegmentSideAttributes
                            {
                                StreetNameId = reader.GetNullableInt32(17),
                                StreetName = reader.GetNullableString(18),
                                MunicipalityNISCode = reader.GetNullableString(19),
                                Municipality = reader.GetNullableString(20)
                            },
                            RecordingDate = reader.GetDateTime(21),
                            Origin = new ImportedOriginProperties
                            {
                                OrganizationId = reader.GetNullableString(22),
                                Organization = reader.GetNullableString(23),
                                Operator = reader.GetNullableString(24),
                                Application = reader.GetNullableString(25),
                                Since = reader.GetDateTime(26),
                                TransactionId = reader.GetNullableInt32(27) ?? TransactionId.Unknown.ToInt32()
                            },
                            PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttributes>(),
                            PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttributes>(),
                            PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttributes>(),
                            Lanes = Array.Empty<ImportedRoadSegmentLaneAttributes>(),
                            Widths = Array.Empty<ImportedRoadSegmentWidthAttributes>(),
                            Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttributes>(),
                            When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                        });
                    });
                return events.ToArray();
            }

            public bool MoveNext()
            {
                if (_state == State.Initial)
                {
                    _batch = ReadInitialBatch();

                    if (_batch.Length == 0)
                    {
                        _state = State.Final;
                        return false;
                    }

                    _state = State.Read;
                    _index = 0;
                    return true;
                }

                if (_state == State.Read)
                {
                    if (_index + 1 < _batch.Length)
                    {
                        _index++;
                        return true;
                    }

                    _batch = ReadNextBatch(_batch[_batch.Length - 1].Id);

                    if (_batch.Length == 0)
                    {
                        _state = State.Final;
                        _index = -1;
                        return false;
                    }

                    _index = 0;
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                _state = State.Initial;
                _batch = Array.Empty<ImportedRoadSegment>();
                _index = -1;
            }

            public ImportedRoadSegment Current
            {
                get
                {
                    if (_state == State.Initial)
                    {
                        throw new InvalidOperationException("Enumeration has not started. Please call MoveNext() first.");
                    }

                    if (_state == State.Final)
                    {
                        throw new InvalidOperationException("Enumeration has ended. Please call Reset() first.");
                    }

                    return _batch[_index];
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public IEnumerator<ImportedRoadSegment> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }
        }

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            var watch = Stopwatch.StartNew();
            foreach (var batch in new RoadSegmentEnumerator(connection, _clock, _reader, _logger).Batch(1000))
            {
                _logger.LogDebug("Reading a road segment batch took {0}ms", watch.ElapsedMilliseconds);

                var lookup = batch.ToDictionary(@event => @event.Id);

                watch.Restart();
                EnrichImportedRoadSegmentsWithEuropeanRoadAttributes(connection, lookup);
                _logger.LogDebug("Reading a road segment batch with european road attributes took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                EnrichImportedRoadSegmentsWithNationalRoadAttributes(connection, lookup);
                _logger.LogDebug("Reading a road segment batch with national road attributes took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                EnrichImportedRoadSegmentsWithNumberedRoadAttributes(connection, lookup);
                _logger.LogDebug("Reading a road segment batch with numbered road attributes took {0}ms", watch.ElapsedMilliseconds);

                watch.Restart();
                EnrichImportedRoadSegmentsWithLaneAttributes(connection, lookup);
                _logger.LogDebug("Reading a road segment batch with lane attributes took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                EnrichImportedRoadSegmentsWithWidthAttributes(connection, lookup);
                _logger.LogDebug("Reading a road segment batch with width attributes took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                EnrichImportedRoadSegmentsWithSurfaceAttributes(connection, lookup);
                _logger.LogDebug("Reading a road segment batch with surface attributes took {0}ms", watch.ElapsedMilliseconds);

                watch.Restart();
                foreach (var @event in batch)
                {
                    yield return new StreamEvent(RoadNetworks.Stream, @event);
                }
                _logger.LogDebug("Yielding a road segment batch took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
            }
            watch.Stop();
        }

        private static SqlParameter[] LookupToParameters(IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            return lookup.Select((item, index) => new SqlParameter("@P" + index, item.Key)).ToArray();
        }

        private static string LookupToParameterNames(IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            return string.Join(',', Enumerable
                .Range(0, lookup.Count - 1)
                .Select(index => "@P" + index)
            );
        }

        private void EnrichImportedRoadSegmentsWithSurfaceAttributes(SqlConnection connection, IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            var command = new SqlCommand(
                @"SELECT wv.[wegsegmentID] --0
                            ,wv.[wegverhardingID] --1
                            ,wv.[type] --2
                            ,wv.[vanPositie] --3
                            ,wv.[totPositie] --4
                            ,wv.[geometrieversie] --5
                            ,wv.[beginorganisatie] --6
                            ,lo.[label] --7
                            ,wv.[begintijd] --8
                        FROM [dbo].[wegverharding] wv
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wv.[beginorganisatie] = lo.[code]
                        WHERE wv.[wegsegmentID] IN (" + LookupToParameterNames(lookup) + @")
                        ORDER BY wv.[wegsegmentID], wv.[vanPositie]",
                connection
            );
            command.Parameters.AddRange(LookupToParameters(lookup));
            command.ForEachDataRecord(reader =>
            {
                var segment = reader.GetInt32(0);
                _logger.LogDebug("Enriching road segment {0} with surface attributes", segment);
                if (!lookup.TryGetValue(segment, out var @event))
                    return;

                var attributes = new ImportedRoadSegmentSurfaceAttributes
                {
                    AttributeId = reader.GetInt32(1),
                    Type = RoadSegmentSurfaceType.ByIdentifier[reader.GetInt32(2)],
                    FromPosition = reader.GetDecimal(3),
                    ToPosition = reader.GetDecimal(4),
                    AsOfGeometryVersion = reader.GetInt32(5),
                    Origin = new ImportedOriginProperties
                    {
                        OrganizationId = reader.GetNullableString(6),
                        Organization = reader.GetNullableString(7),
                        Since = reader.GetDateTime(8)
                    }
                };

                var copy = new ImportedRoadSegmentSurfaceAttributes[@event.Surfaces.Length + 1];
                @event.Surfaces.CopyTo(copy, 0);
                copy[@event.Surfaces.Length] = attributes;
                @event.Surfaces = copy;
            });
        }

        private void EnrichImportedRoadSegmentsWithWidthAttributes(SqlConnection connection, IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            var command = new SqlCommand(
                @"SELECT wb.[wegsegmentID] --0
                            ,wb.[wegbreedteID] --1
                            ,wb.[breedte] --2
                            ,wb.[vanPositie] --3
                            ,wb.[totPositie] --4
                            ,wb.[geometrieversie] --5
                            ,wb.[beginorganisatie] --6
                            ,lo.[label] --7
                            ,wb.[begintijd] --8
                        FROM [dbo].[wegbreedte] wb
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wb.[beginorganisatie] = lo.[code]
                        WHERE wb.[wegsegmentID] IN (" + LookupToParameterNames(lookup) + @")
                        ORDER BY wb.[wegsegmentID], wb.[vanPositie]",
                connection
            );
            command.Parameters.AddRange(LookupToParameters(lookup));
            command.ForEachDataRecord(reader =>
            {
                var segment = reader.GetInt32(0);
                _logger.LogDebug("Enriching road segment {0} with width attributes", segment);
                if (!lookup.TryGetValue(segment, out var @event))
                    return;

                var attributes = new ImportedRoadSegmentWidthAttributes
                {
                    AttributeId = reader.GetInt32(1),
                    Width = reader.GetInt32(2),
                    FromPosition = reader.GetDecimal(3),
                    ToPosition = reader.GetDecimal(4),
                    AsOfGeometryVersion = reader.GetInt32(5),
                    Origin = new ImportedOriginProperties
                    {
                        OrganizationId = reader.GetNullableString(6),
                        Organization = reader.GetNullableString(7),
                        Since = reader.GetDateTime(8)
                    }
                };

                var copy = new ImportedRoadSegmentWidthAttributes[@event.Widths.Length + 1];
                @event.Widths.CopyTo(copy, 0);
                copy[@event.Widths.Length] = attributes;
                @event.Widths = copy;
            });
        }

        private void EnrichImportedRoadSegmentsWithLaneAttributes(SqlConnection connection, IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            var command = new SqlCommand(
                @"SELECT
                            rs.[wegsegmentID] --0
                            ,rs.[rijstrokenID] --1
                            ,rs.[aantal] --2
                            ,rs.[richting] --3
                            ,rs.[vanPositie] --4
                            ,rs.[totPositie] --5
                            ,rs.[geometrieversie] --6
                            ,rs.[beginorganisatie] --7
                            ,lo.[label] --8
                            ,rs.[begintijd] --9
                        FROM [dbo].[rijstroken] rs
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON rs.[beginorganisatie] = lo.[code]
                        WHERE rs.[wegsegmentID] IN (" + LookupToParameterNames(lookup) + @")
                        ORDER BY rs.[wegsegmentID], rs.[vanPositie]",
                connection
            );
            command.Parameters.AddRange(LookupToParameters(lookup));
            command.ForEachDataRecord(reader =>
            {
                var segment = reader.GetInt32(0);
                _logger.LogDebug("Enriching road segment {0} with lane attributes", segment);
                if (!lookup.TryGetValue(segment, out var @event))
                    return;

                var attributes = new ImportedRoadSegmentLaneAttributes
                {
                    AttributeId = reader.GetInt32(1),
                    Count = reader.GetInt32(2),
                    Direction = RoadSegmentLaneDirection.ByIdentifier[reader.GetInt32(3)],
                    FromPosition = reader.GetDecimal(4),
                    ToPosition = reader.GetDecimal(5),
                    AsOfGeometryVersion = reader.GetInt32(6),
                    Origin = new ImportedOriginProperties
                    {
                        OrganizationId = reader.GetNullableString(7),
                        Organization = reader.GetNullableString(8),
                        Since = reader.GetDateTime(9)
                    }
                };

                var copy = new ImportedRoadSegmentLaneAttributes[@event.Lanes.Length + 1];
                @event.Lanes.CopyTo(copy, 0);
                copy[@event.Lanes.Length] = attributes;
                @event.Lanes = copy;
            });
        }

        private void EnrichImportedRoadSegmentsWithNumberedRoadAttributes(SqlConnection connection, IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            var command = new SqlCommand(
                @"SELECT
                            gw.[wegsegmentID]
                            ,gw.[genummerdeWegID]
                            ,gw.[ident8]
                            ,gw.[richting]
                            ,gw.[volgnummer]
                            ,gw.[beginorganisatie]
                            ,lo.[label]
                            ,gw.[begintijd]
                        FROM [dbo].[genummerdeWeg] gw
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON gw.[beginorganisatie] = lo.[code]
                        WHERE gw.[wegsegmentID] IS NOT NULL
                        AND gw.[wegsegmentID] IN (" + LookupToParameterNames(lookup) + ")",
                connection
            );
            command.Parameters.AddRange(LookupToParameters(lookup));
            command.ForEachDataRecord(reader =>
            {
                var segment = reader.GetInt32(0);
                _logger.LogDebug("Enriching road segment {0} with numbered road attributes", segment);
                if (!lookup.TryGetValue(segment, out var @event))
                    return;

                var attributes = new ImportedRoadSegmentNumberedRoadAttributes
                {
                    AttributeId = reader.GetInt32(1),
                    Ident8 = reader.GetString(2),
                    Direction = RoadSegmentNumberedRoadDirection.ByIdentifier[reader.GetInt32(3)],
                    Ordinal = reader.GetInt32(4),
                    Origin = new ImportedOriginProperties
                    {
                        OrganizationId = reader.GetNullableString(5),
                        Organization = reader.GetNullableString(6),
                        Since = reader.GetDateTime(7)
                    }
                };

                var copy = new ImportedRoadSegmentNumberedRoadAttributes[@event.PartOfNumberedRoads.Length + 1];
                @event.PartOfNumberedRoads.CopyTo(copy, 0);
                copy[@event.PartOfNumberedRoads.Length] = attributes;
                @event.PartOfNumberedRoads = copy;
            });
        }

        private void EnrichImportedRoadSegmentsWithNationalRoadAttributes(SqlConnection connection, IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            var command = new SqlCommand(
                @"SELECT nw.[wegsegmentID]
                            ,nw.[nationaleWegID]
                            ,nw.[ident2]
                            ,nw.[beginorganisatie]
                            ,lo.[label]
                            ,nw.[begintijd]
                        FROM [dbo].[nationaleWeg] nw
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON nw.[beginorganisatie] = lo.[code]
                        WHERE nw.[wegsegmentID] IS NOT NULL
                        AND nw.[wegsegmentID] IN (" + LookupToParameterNames(lookup) + ")",
                connection
            );
            command.Parameters.AddRange(LookupToParameters(lookup));
            command.ForEachDataRecord(reader =>
            {
                var segment = reader.GetInt32(0);
                _logger.LogDebug("Enriching road segment {0} with national road attributes", segment);
                if (!lookup.TryGetValue(segment, out var @event))
                    return;

                var attributes = new ImportedRoadSegmentNationalRoadAttributes
                {
                    AttributeId = reader.GetInt32(1),
                    Ident2 = reader.GetString(2),
                    Origin = new ImportedOriginProperties
                    {
                        OrganizationId = reader.GetNullableString(3),
                        Organization = reader.GetNullableString(4),
                        Since = reader.GetDateTime(5)
                    }
                };

                var copy = new ImportedRoadSegmentNationalRoadAttributes[@event.PartOfNationalRoads.Length + 1];
                @event.PartOfNationalRoads.CopyTo(copy, 0);
                copy[@event.PartOfNationalRoads.Length] = attributes;
                @event.PartOfNationalRoads = copy;
            });
        }

        private void EnrichImportedRoadSegmentsWithEuropeanRoadAttributes(SqlConnection connection, IReadOnlyDictionary<int, ImportedRoadSegment> lookup)
        {
            var command = new SqlCommand(
                @"SELECT ew.[wegsegmentID]
                            ,ew.[EuropeseWegID]
                            ,ew.[euWegnummer]
                            ,ew.[beginorganisatie]
                            ,lo.[label]
                            ,ew.[begintijd]
                        FROM [dbo].[EuropeseWeg] ew
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ew.[beginorganisatie] = lo.[code]
                        WHERE ew.[wegsegmentID] IS NOT NULL
                        AND ew.[wegsegmentID] IN (" + LookupToParameterNames(lookup) + ")",
                connection
            );
            command.Parameters.AddRange(LookupToParameters(lookup));
            command.ForEachDataRecord(reader =>
            {
                var segment = reader.GetInt32(0);
                _logger.LogDebug("Enriching road segment {0} with european road attributes", segment);
                if (!lookup.TryGetValue(segment, out var @event))
                    return;

                var attributes = new ImportedRoadSegmentEuropeanRoadAttributes
                {
                    AttributeId = reader.GetInt32(1),
                    Number = reader.GetString(2),
                    Origin = new ImportedOriginProperties
                    {
                        OrganizationId = reader.GetNullableString(3),
                        Organization = reader.GetNullableString(4),
                        Since = reader.GetDateTime(5)
                    }
                };

                var copy = new ImportedRoadSegmentEuropeanRoadAttributes[@event.PartOfEuropeanRoads.Length + 1];
                @event.PartOfEuropeanRoads.CopyTo(copy, 0);
                copy[@event.PartOfEuropeanRoads.Length] = attributes;
                @event.PartOfEuropeanRoads = copy;
            });
        }
    }
}
