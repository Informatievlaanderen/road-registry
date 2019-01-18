namespace RoadRegistry.LegacyStreamExtraction
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Amazon.S3;
    using BackOffice.Messages;
    using GeoAPI.Geometries;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        const string EXPORT_TO_LOCAL_FILE = "ExportToLocalFile";
        const string LEGACY_STREAM_FILE_NAME = "LegacyStreamFileName";
        const string LEGACY_STREAM_FILE_BUCKET = "LegacyStreamFileBucket";

        private static async Task Main(string[] args)
        {

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var wkbReader = new WellKnownBinaryReader();

            var root = configurationBuilder.Build();
            var output = new FileInfo(root[LEGACY_STREAM_FILE_NAME]);
            var connectionString = root.GetConnectionString("Legacy");
            var importedRoadNodes = new List<ImportedRoadNode>();
            var importedRoadSegments = new Dictionary<long, ImportedRoadSegment>();
            var importedGradeSeparatedJunctions = new List<ImportedGradeSeparatedJunction>();
            var importedOrganizations = new List<ImportedOrganization>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                Console.WriteLine("Reading organizations started ...");
                var watch = Stopwatch.StartNew();
                await
                    new SqlCommand(
                        @"SELECT
                            org.[Code],
                            org.[Label]
                        FROM [listOrganisatie] org", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var organization = new ImportedOrganization
                        {
                            Code = reader.GetString(0),
                            Name = reader.GetString(1)
                        };
                        importedOrganizations.Add(organization);
                    });
                Console.WriteLine("Reading organizations took {0}ms", watch.ElapsedMilliseconds);

                Console.WriteLine("Reading nodes started ...");
                watch.Restart();
                await
                    new SqlCommand(
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
                    ).ForEachDataRecord(reader =>
                    {
                        var wellKnownBinary = reader.GetAllBytes(3);
                        var geometry = wkbReader.ReadAs<NetTopologySuite.Geometries.Point>(wellKnownBinary);
                        var node = new ImportedRoadNode
                        {
                            Id = reader.GetInt32(0),
                            Version = reader.GetInt32(1),
                            Type = Translate.ToRoadNodeType(reader.GetInt32(2)),
                            Geometry = new RoadNodeGeometry
                            {
                                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
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
                            }
                        };
                        importedRoadNodes.Add(node);
                    });
                Console.WriteLine("Reading nodes took {0}ms", watch.ElapsedMilliseconds);

                Console.WriteLine("Reading segments started ...");
                watch.Restart();
                await
                    new SqlCommand(
                        @"SELECT
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
                            ws.[begintijd] --24
                        FROM [dbo].[wegsegment] ws
                        LEFT OUTER JOIN [dbo].[gemeenteNIS] lg ON ws.[linksGemeente] = lg.[gemeenteId]
                        LEFT OUTER JOIN [dbo].[crabsnm] ls ON ws.[linksStraatnaamID] = ls.[EXN]
                        LEFT OUTER JOIN [dbo].[gemeenteNIS] rg ON ws.[rechtsGemeente] = rg.[gemeenteId]
                        LEFT OUTER JOIN [dbo].[crabsnm] rs ON ws.[rechtsStraatnaamID] = rs.[EXN]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ws.[beginorganisatie] = lo.[code]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] beheerders ON ws.[beheerder] = beheerders.[code]",
                        connection
                    ).ForEachDataRecord(reader =>
                    {
                        var wellKnownBinary = reader.GetAllBytes(4);
                        var geometry = wkbReader
                            .TryReadAs(wellKnownBinary, out NetTopologySuite.Geometries.LineString oneLine)
                            ? new NetTopologySuite.Geometries.MultiLineString(new ILineString[] {oneLine})
                            : wkbReader.ReadAs<NetTopologySuite.Geometries.MultiLineString>(wellKnownBinary);

                        var multiLineString = Array.ConvertAll(
                            geometry.Geometries.Cast<NetTopologySuite.Geometries.LineString>().ToArray(),
                            input => new LineString
                            {
                                Points = Array.ConvertAll(
                                    input.Coordinates,
                                    coordinate => new Point
                                    {
                                        X = coordinate.X,
                                        Y = coordinate.Y
                                    }),
                                Measures = geometry.GetOrdinates(Ordinate.M)
                            });

                        var segment = new ImportedRoadSegment
                        {
                            Id = reader.GetInt32(0),
                            Version = reader.GetInt32(1),
                            StartNodeId = reader.GetInt32(2),
                            EndNodeId = reader.GetInt32(3),
                            Geometry = new RoadSegmentGeometry
                            {
                                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32(),
                                MultiLineString = multiLineString
                            },
                            GeometryVersion = reader.GetInt32(5),
                            MaintenanceAuthority = new MaintenanceAuthority
                            {
                                Code = reader.GetString(6),
                                Name = reader.GetString(7)
                            },
                            GeometryDrawMethod = Translate.ToRoadSegmentGeometryDrawMethod(reader.GetInt32(8)),
                            Morphology = Translate.ToRoadSegmentMorphology(reader.GetInt32(9)),
                            Status = Translate.ToRoadSegmentStatus(reader.GetInt32(10)),
                            Category = Translate.ToRoadSegmentCategory(reader.GetString(11)),
                            AccessRestriction = Translate.ToRoadSegmentAccessRestriction(reader.GetInt32(12)),
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
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(22),
                                Organization = reader.GetNullableString(23),
                                Since = reader.GetDateTime(24)
                            },
                            PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttributes>(),
                            PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttributes>(),
                            PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttributes>(),
                            Lanes = Array.Empty<ImportedRoadSegmentLaneAttributes>(),
                            Widths = Array.Empty<ImportedRoadSegmentWidthAttributes>(),
                            Surfaces = Array.Empty<ImportedRoadSegmentSurfaceAttributes>(),
                        };
                        importedRoadSegments.Add(segment.Id, segment);
                    });
                Console.WriteLine("Reading segments took {0}ms", watch.ElapsedMilliseconds);

                Console.WriteLine("Enriching segments with european road information ...");
                watch.Restart();
                await
                    new SqlCommand(
                        @"SELECT ew.[wegsegmentID]
                            ,ew.[EuropeseWegID]
                            ,ew.[euWegnummer]
                            ,ew.[beginorganisatie]
                            ,lo.[label]
                            ,ew.[begintijd]
                        FROM [dbo].[EuropeseWeg] ew
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ew.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!importedRoadSegments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new ImportedRoadSegmentEuropeanRoadAttributes
                        {
                            AttributeId = reader.GetInt32(1),
                            Number = reader.GetString(2),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(3),
                                Organization = reader.GetNullableString(4),
                                Since = reader.GetDateTime(5)
                            }
                        };

                        var copy = new ImportedRoadSegmentEuropeanRoadAttributes[segment.PartOfEuropeanRoads.Length + 1];
                        segment.PartOfEuropeanRoads.CopyTo(copy, 0);
                        copy[segment.PartOfEuropeanRoads.Length] = props;
                        segment.PartOfEuropeanRoads = copy;
                    });

                Console.WriteLine("Enriching segments with national road information ...");
                await
                    new SqlCommand(
                        @"SELECT nw.[wegsegmentID]
                            ,nw.[nationaleWegID]
                            ,nw.[ident2]
                            ,nw.[beginorganisatie]
                            ,lo.[label]
                            ,nw.[begintijd]
                        FROM [dbo].[nationaleWeg] nw
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON nw.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!importedRoadSegments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new ImportedRoadSegmentNationalRoadAttributes
                        {
                            AttributeId = reader.GetInt32(1),
                            Ident2 = reader.GetString(2),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(3),
                                Organization = reader.GetNullableString(4),
                                Since = reader.GetDateTime(5)
                            }
                        };

                        var copy = new ImportedRoadSegmentNationalRoadAttributes[segment.PartOfNationalRoads.Length + 1];
                        segment.PartOfNationalRoads.CopyTo(copy, 0);
                        copy[segment.PartOfNationalRoads.Length] = props;
                        segment.PartOfNationalRoads = copy;
                    });

                Console.WriteLine("Enriching segments with numbered road information ...");
                await
                    new SqlCommand(
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
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON gw.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!importedRoadSegments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new ImportedRoadSegmentNumberedRoadAttributes
                        {
                            AttributeId = reader.GetInt32(1),
                            Ident8 = reader.GetString(2),
                            Direction = Translate.ToNumberedRoadSegmentDirection(reader.GetInt32(3)),
                            Ordinal = reader.GetInt32(4),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(5),
                                Organization = reader.GetNullableString(6),
                                Since = reader.GetDateTime(7)
                            }
                        };

                        var copy = new ImportedRoadSegmentNumberedRoadAttributes[segment.PartOfNumberedRoads.Length + 1];
                        segment.PartOfNumberedRoads.CopyTo(copy, 0);
                        copy[segment.PartOfNumberedRoads.Length] = props;
                        segment.PartOfNumberedRoads = copy;
                    });

                Console.WriteLine("Enriching segments with lane information ...");
                await
                    new SqlCommand(
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
                        INNER JOIN [dbo].[wegsegment] ws ON rs.[wegsegmentID] = ws.[wegsegmentID]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON rs.[beginorganisatie] = lo.[code]
                        ORDER BY rs.[wegsegmentID], rs.[vanPositie]",
                        connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!importedRoadSegments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new ImportedRoadSegmentLaneAttributes
                        {
                            AttributeId = reader.GetInt32(1),
                            Count = reader.GetInt32(2),
                            Direction = Translate.ToLaneDirection(reader.GetInt32(3)),
                            FromPosition = reader.GetDecimal(4),
                            ToPosition = reader.GetDecimal(5),
                            AsOfGeometryVersion = reader.GetInt32(6),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(7),
                                Organization = reader.GetNullableString(8),
                                Since = reader.GetDateTime(9)
                            }
                        };

                        var copy = new ImportedRoadSegmentLaneAttributes[segment.Lanes.Length + 1];
                        segment.Lanes.CopyTo(copy, 0);
                        copy[segment.Lanes.Length] = props;
                        segment.Lanes = copy;
                    });

                Console.WriteLine("Enriching segments with width information ...");
                await
                    new SqlCommand(
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
                        INNER JOIN [dbo].[wegsegment] ws ON wb.[wegsegmentID] = ws.[wegsegmentID]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wb.[beginorganisatie] = lo.[code]
                        ORDER BY wb.[wegsegmentID], wb.[vanPositie]",
                        connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!importedRoadSegments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new ImportedRoadSegmentWidthAttributes
                        {
                            AttributeId = reader.GetInt32(1),
                            Width = reader.GetInt32(2),
                            FromPosition = reader.GetDecimal(3),
                            ToPosition = reader.GetDecimal(4),
                            AsOfGeometryVersion = reader.GetInt32(5),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(6),
                                Organization = reader.GetNullableString(7),
                                Since = reader.GetDateTime(8)
                            }
                        };

                        var copy = new ImportedRoadSegmentWidthAttributes[segment.Widths.Length + 1];
                        segment.Widths.CopyTo(copy, 0);
                        copy[segment.Widths.Length] = props;
                        segment.Widths = copy;
                    });

                Console.WriteLine("Enriching segments with surface information ...");
                await
                    new SqlCommand(
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
                        INNER JOIN [dbo].[wegsegment] ws ON wv.[wegsegmentID] = ws.[wegsegmentID]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wv.[beginorganisatie] = lo.[code]
                        ORDER BY wv.[wegsegmentID], wv.[vanPositie]",
                        connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!importedRoadSegments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new ImportedRoadSegmentSurfaceAttributes
                        {
                            AttributeId = reader.GetInt32(1),
                            Type = Translate.ToSurfaceType(reader.GetInt32(2)),
                            FromPosition = reader.GetDecimal(3),
                            ToPosition = reader.GetDecimal(4),
                            AsOfGeometryVersion = reader.GetInt32(5),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(6),
                                Organization = reader.GetNullableString(7),
                                Since = reader.GetDateTime(8)
                            }
                        };

                        var copy = new ImportedRoadSegmentSurfaceAttributes[segment.Surfaces.Length + 1];
                        segment.Surfaces.CopyTo(copy, 0);
                        copy[segment.Surfaces.Length] = props;
                        segment.Surfaces = copy;
                    });
                Console.WriteLine("Enriching segments took {0}ms", watch.ElapsedMilliseconds);

                Console.WriteLine("Reading junctions started ...");
                watch.Restart();
                await
                    new SqlCommand(
                        @"SELECT ok.[ongelijkgrondseKruisingID]
                            ,ok.[bovenWegsegmentID]
                            ,ok.[onderWegsegmentID]
                            ,ok.[type]
                            ,ok.[beginorganisatie]
                            ,lo.[label]
                            ,ok.[begintijd]
                        FROM [wegenregister].[dbo].[ongelijkgrondseKruising] ok
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ok.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var junction = new ImportedGradeSeparatedJunction
                        {
                            Id = reader.GetInt32(0),
                            UpperRoadSegmentId = reader.GetInt32(1),
                            LowerRoadSegmentId = reader.GetInt32(2),
                            Type = Translate.ToGradeSeparatedJunctionType(reader.GetInt32(3)),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(4),
                                Organization = reader.GetNullableString(5),
                                Since = reader.GetDateTime(6)
                            }
                        };

                        importedGradeSeparatedJunctions.Add(junction);
                    });
                Console.WriteLine("Reading junctions took {0}ms", watch.ElapsedMilliseconds);

                connection.Close();

                Console.WriteLine("Writing stream to json started ...");
                watch.Restart();
                await new ExtractedStreamsWriter(output)
                    .WriteAsync(importedOrganizations, importedRoadNodes, importedRoadSegments.Values, importedGradeSeparatedJunctions);
                Console.WriteLine("Writing stream to json took {0}ms", watch.ElapsedMilliseconds);


                if (UploadToS3(root))
                {
                    Console.WriteLine("Upload to S3 started ...");
                    watch.Restart();
                    var bucketName = root[LEGACY_STREAM_FILE_BUCKET];
                    try
                    {
                        var s3Client = root
                            .GetAWSOptions()
                            .CreateServiceClient<IAmazonS3>();

                        await s3Client.UploadObjectFromStreamAsync(
                            bucketName,
                            output.Name,
                            output.OpenRead(),
                            new Dictionary<string, object>(),
                            CancellationToken.None
                        );

                        File.Delete(output.FullName);
                        Console.WriteLine($"Upload {output.Name} to S3:{bucketName}/{output.Name} took {watch.ElapsedMilliseconds}ms");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"Upload {output.FullName} to S3:{bucketName}/{output.Name} failed after {watch.ElapsedMilliseconds}ms: {exception}");
                    }
                }
            }
        }

        private static bool UploadToS3(IConfiguration root)
        {
            return false == root.GetValue<bool>(EXPORT_TO_LOCAL_FILE);
        }
    }
}
