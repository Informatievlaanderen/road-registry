namespace RoadRegistry.LegacyImporter
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Events;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;

    public class Program
    {
        private static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var connectionString = configurationBuilder.Build().GetConnectionString("Legacy");
            var nodes = new List<ImportedRoadNode>();
            var points = new List<ImportedReferencePoint>();
            var segments = new Dictionary<int, ImportedRoadSegment>();
            var junctions = new List<ImportedGradeSeparatedJunction>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                Console.WriteLine("Reading nodes started ...");
                var watch = Stopwatch.StartNew();
                await
                    new SqlCommand(
                        @"SELECT 
                            wk.[wegknoopID],
                            wk.[type],
                            wk.[geometrie].STAsBinary(),
                            wk.[beginorganisatie],
                            lo.[label],
                            wk.[begintijd]
                        FROM [wegknoop] wk
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wk.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var node = new ImportedRoadNode
                        {
                            Id = reader.GetInt32(0),
                            Type = Translate.ToRoadNodeType(reader.GetInt32(1)),
                            WellKnownBinaryGeometry = reader.GetAllBytes(2),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(3),
                                Organization = reader.GetNullableString(4),
                                Since = reader.GetDateTime(5)
                            }
                        };

                        nodes.Add(node);
                    });
                Console.WriteLine("Reading nodes took {0}ms", watch.ElapsedMilliseconds);

                Console.WriteLine("Reading segments started ...");
                watch.Restart();
                await
                    new SqlCommand(
                        @"SELECT 
                            ws.[wegsegmentID], --0
                            ws.[beginWegknoopID], --1
                            ws.[eindWegknoopID], --2
                            ws.[geometrie].STAsBinary(), --3
                            ws.[beheerder], --4
                            ws.[methode], --5
                            ws.[morfologie], --6
                            ws.[status], --7
                            ws.[categorie], --8
                            ws.[toegangsbeperking], --9
                            ws.[linksStraatnaamID], --10
                            lg.[NISCode], --11
                            ws.[rechtsStraatnaamID], --12
                            rg.[NISCode], --13
                            ws.[opnamedatum], --14
                            ws.[beginorganisatie], --15
                            lo.[label], --16
                            ws.[begintijd] --17
                        FROM [dbo].[wegsegment] ws
                        LEFT OUTER JOIN [dbo].[gemeenteNIS] lg ON ws.[linksGemeente] = lg.[gemeenteId]
                        LEFT OUTER JOIN [dbo].[gemeenteNIS] rg ON ws.[rechtsGemeente] = rg.[gemeenteId]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ws.[beginorganisatie] = lo.[code]
                        WHERE ws.[eindWegknoopID] IS NOT NULL", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segment = new ImportedRoadSegment
                        {
                            Id = reader.GetInt32(0),
                            StartNodeId = reader.GetInt32(1),
                            EndNodeId = reader.GetInt32(2),
                            WellKnownBinaryGeometry = reader.GetAllBytes(3),
                            MaintainerId = reader.GetString(4),
                            GeometryDrawMethod = Translate.ToRoadSegmentGeometryDrawMethod(reader.GetInt32(5)),
                            Morphology = Translate.ToRoadSegmentMorphology(reader.GetInt32(6)),
                            Status = Translate.ToRoadSegmentStatus(reader.GetInt32(7)),
                            Category = Translate.ToRoadSegmentCategory(reader.GetString(8)),
                            AccessRestriction = Translate.ToRoadSegmentAccessRestriction(reader.GetInt32(9)),
                            LeftSide = new RoadSegmentSideProperties
                            {
                                StreetNameId = reader.GetNullableInt32(10),
                                MunicipalityNISCode = reader.GetNullableString(11)
                            },
                            RightSide = new RoadSegmentSideProperties
                            {
                                StreetNameId = reader.GetNullableInt32(12),
                                MunicipalityNISCode = reader.GetNullableString(13)
                            },
                            RecordingDate = reader.GetDateTime(14),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(15),
                                Organization = reader.GetNullableString(16),
                                Since = reader.GetDateTime(17)
                            },
                            PartOfEuropeanRoads = Array.Empty<RoadSegmentEuropeanRoadProperties>(),
                            PartOfNationalRoads = Array.Empty<RoadSegmentNationalRoadProperties>(),
                            PartOfNumberedRoads = Array.Empty<RoadSegmentNumberedRoadProperties>(),
                            Lanes = Array.Empty<RoadSegmentLaneProperties>(),
                            Widths = Array.Empty<RoadSegmentWidthProperties>(),
                            Hardenings = Array.Empty<RoadSegmentHardeningProperties>(),
                        };

                        segments.Add(segment.Id, segment);
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
                        if (!segments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new RoadSegmentEuropeanRoadProperties
                        {
                            AttributeId = reader.GetInt32(1),
                            RoadNumber = reader.GetString(2),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(3),
                                Organization = reader.GetNullableString(4),
                                Since = reader.GetDateTime(5)
                            }
                        };

                        var copy = new RoadSegmentEuropeanRoadProperties[segment.PartOfEuropeanRoads.Length + 1];
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
                        if (!segments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new RoadSegmentNationalRoadProperties
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

                        var copy = new RoadSegmentNationalRoadProperties[segment.PartOfNationalRoads.Length + 1];
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
                        if (!segments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new RoadSegmentNumberedRoadProperties
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

                        var copy = new RoadSegmentNumberedRoadProperties[segment.PartOfNumberedRoads.Length + 1];
                        segment.PartOfNumberedRoads.CopyTo(copy, 0);
                        copy[segment.PartOfNumberedRoads.Length] = props;
                        segment.PartOfNumberedRoads = copy;
                    });

                Console.WriteLine("Enriching segments with lane information ...");
                await
                    new SqlCommand(
                        @"SELECT 
                            rs.[wegsegmentID]
                            ,rs.[rijstrokenID]
                            ,rs.[aantal]
                            ,rs.[richting]
                            ,rs.[vanPositie]
                            ,rs.[totPositie]
                            ,rs.[beginorganisatie]
                            ,lo.[label]
                            ,rs.[begintijd]
                        FROM [dbo].[rijstroken] rs
                        INNER JOIN [dbo].[wegsegment] ws ON rs.[wegsegmentID] = ws.[wegsegmentID] AND rs.[geometrieversie] = ws.[geometrieversie]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON rs.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!segments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new RoadSegmentLaneProperties
                        {
                            AttributeId = reader.GetInt32(1),
                            Count = reader.GetInt32(2),
                            Direction = Translate.ToLaneDirection(reader.GetInt32(3)),
                            FromPosition = reader.GetDecimal(4),
                            ToPosition = reader.GetDecimal(5),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(6),
                                Organization = reader.GetNullableString(7),
                                Since = reader.GetDateTime(8)
                            }
                        };

                        var copy = new RoadSegmentLaneProperties[segment.Lanes.Length + 1];
                        segment.Lanes.CopyTo(copy, 0);
                        copy[segment.Lanes.Length] = props;
                        segment.Lanes = copy;
                    });

                Console.WriteLine("Enriching segments with width information ...");
                await
                    new SqlCommand(
                        @"SELECT wb.[wegsegmentID]
                            ,wb.[wegbreedteID]
                            ,wb.[breedte]
                            ,wb.[vanPositie]
                            ,wb.[totPositie]
                            ,wb.[beginorganisatie]
                            ,lo.[label]
                            ,wb.[begintijd]
                        FROM [dbo].[wegbreedte] wb
                        INNER JOIN [dbo].[wegsegment] ws ON wb.[wegsegmentID] = ws.[wegsegmentID] AND wb.[geometrieversie] = ws.[geometrieversie]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wb.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!segments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new RoadSegmentWidthProperties
                        {
                            AttributeId = reader.GetInt32(1),
                            Width = reader.GetInt32(2),
                            FromPosition = reader.GetDecimal(3),
                            ToPosition = reader.GetDecimal(4),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(5),
                                Organization = reader.GetNullableString(6),
                                Since = reader.GetDateTime(7)
                            }
                        };

                        var copy = new RoadSegmentWidthProperties[segment.Widths.Length + 1];
                        segment.Widths.CopyTo(copy, 0);
                        copy[segment.Widths.Length] = props;
                        segment.Widths = copy;
                    });

                Console.WriteLine("Enriching segments with hardening information ...");
                await
                    new SqlCommand(
                        @"SELECT wv.[wegsegmentID]
                            ,wv.[wegverhardingID]
                            ,wv.[type]
                            ,wv.[vanPositie]
                            ,wv.[totPositie]
                            ,wv.[beginorganisatie]
                            ,lo.[label]
                            ,wv.[begintijd]
                        FROM [dbo].[wegverharding] wv
                        INNER JOIN [dbo].[wegsegment] ws ON wv.[wegsegmentID] = ws.[wegsegmentID] AND wv.[geometrieversie] = ws.[geometrieversie]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wv.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if (!segments.TryGetValue(segmentId, out var segment))
                            return;

                        var props = new RoadSegmentHardeningProperties
                        {
                            AttributeId = reader.GetInt32(1),
                            Type = Translate.ToHardeningType(reader.GetInt32(2)),
                            FromPosition = reader.GetDecimal(3),
                            ToPosition = reader.GetDecimal(4),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(5),
                                Organization = reader.GetNullableString(6),
                                Since = reader.GetDateTime(7)
                            }
                        };

                        var copy = new RoadSegmentHardeningProperties[segment.Hardenings.Length + 1];
                        segment.Hardenings.CopyTo(copy, 0);
                        copy[segment.Hardenings.Length] = props;
                        segment.Hardenings = copy;
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

                        junctions.Add(junction);
                    });
                Console.WriteLine("Reading junctions took {0}ms", watch.ElapsedMilliseconds);

                Console.WriteLine("Reading reference points started ...");
                watch.Restart();
                await
                    new SqlCommand(
                        @"SELECT rp.[referentiepuntID] --0
                            ,rp.[geometrie].STAsBinary() --1
                            ,rp.[ident8] --2
                            ,rp.[type] --3
                            ,CONVERT(NVARCHAR(MAX), rp.[opschrift]) --4
                            ,rp.[beginorganisatie] --5
                            ,lo.[label] --6
                            ,rp.[begintijd] --7
                        FROM [dbo].[referentiepunt] rp
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON rp.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var point = new ImportedReferencePoint
                        {
                            Id = reader.GetInt32(0),
                            WellKnownBinaryGeometry = reader.GetAllBytes(1),
                            Ident8 = reader.GetString(2),
                            Type = Translate.ToReferencePointType(reader.GetInt32(3)),
                            Caption = reader.GetString(4),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(5),
                                Organization = reader.GetNullableString(6),
                                Since = reader.GetDateTime(7)
                            }
                        };

                        points.Add(point);
                    });
                Console.WriteLine("Reading reference points took {0}ms", watch.ElapsedMilliseconds);
                connection.Close();

                Console.WriteLine("Writing stream to json started ...");
                watch.Restart();
                await WriteExtractedStreamToJsonFile(nodes, segments.Values, junctions, points);
                Console.WriteLine("Writing stream to json took {0}ms", watch.ElapsedMilliseconds);
            }
        }

        private static async Task WriteExtractedStreamToJsonFile(
            IEnumerable<ImportedRoadNode> nodes,
            IEnumerable<ImportedRoadSegment> segments,
            IEnumerable<ImportedGradeSeparatedJunction> junctions,
            IEnumerable<ImportedReferencePoint> points)
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = DateParseHandling.DateTime,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            const string output = "roadnetworkstream.json"; // TODO: prefer injecting this as an output parameter
            if (File.Exists(output))
                File.Delete(output);

            using (var stream = File.OpenWrite(output))
            {
                using (var writer = new JsonTextWriter(new StreamWriter(stream)))
                {
                    await writer.WriteStartArrayAsync();

                    foreach (var node in nodes)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedRoadNode));
                        serializer.Serialize(writer, node);
                        await writer.WriteEndObjectAsync();
                    }

                    foreach (var segment in segments)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedRoadSegment));
                        serializer.Serialize(writer, segment);
                        await writer.WriteEndObjectAsync();
                    }

                    foreach (var junction in junctions)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedGradeSeparatedJunction));
                        serializer.Serialize(writer, junction);
                        await writer.WriteEndObjectAsync();
                    }

                    foreach (var point in points)
                    {
                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync(nameof(ImportedReferencePoint));
                        serializer.Serialize(writer, point);
                        await writer.WriteEndObjectAsync();
                    }

                    await writer.WriteEndArrayAsync();
                    await writer.FlushAsync();
                }
            }
        }
    }
}
