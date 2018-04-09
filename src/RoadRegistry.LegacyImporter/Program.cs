using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RoadRegistry.Commands;

namespace RoadRegistry.LegacyImporter
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var connectionString = configurationBuilder.Build().GetConnectionString("Legacy");
            var nodes = new List<RoadNode>();
            var segments = new Dictionary<int, RoadSegment>();
            var junctions = new List<GradeSeparatedJunction>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var before = Process.GetCurrentProcess().VirtualMemorySize64;
                Console.WriteLine("Reading nodes started ...");
                var watch = Stopwatch.StartNew();
                await 
                    new SqlCommand(
                        @"SELECT 
                            wk.[wegknoopID],
                            wk.[type],
                            wk.[geometrie],
                            wk.[beginorganisatie],
                            lo.[label],
                            wk.[begintijd]
                        FROM [wegknoop] wk
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON wk.[beginorganisatie] = lo.[code]", connection
                    )
                    .ExecuteWith(reader => 
                    {
                        var node = new RoadNode
                        {
                            Id = reader.GetInt32(0),
                            Type = Translate.ToRoadNodeType(reader.GetInt32(1)),
                            Geometry = reader.GetAllBytes(2),
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
                watch.Restart();
                Console.WriteLine("Reading segments started ...");
                await 
                    new SqlCommand(
                        @"SELECT 
                            ws.[wegsegmentID], --0
                            ws.[beginWegknoopID], --1
                            ws.[eindWegknoopID], --2
                            ws.[geometrie], --3
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
                    ).ExecuteWith(reader =>
                    {
                        var segment = new RoadSegment
                        {
                            Id = reader.GetInt32(0),
                            StartNodeId = reader.GetInt32(1),
                            EndNodeId = reader.GetInt32(2),
                            Geometry = reader.GetAllBytes(3),
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
                            Surfaces = Array.Empty<RoadSegmentSurfaceProperties>(),
                        };
                        segments.Add(segment.Id, segment);
                    });
                Console.WriteLine("Reading segments took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                Console.WriteLine("Enriching segments with european road information ...");
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
                    ).ExecuteWith(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if(segments.TryGetValue(segmentId, out RoadSegment segment))
                        {
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
                        }
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
                    ).ExecuteWith(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if(segments.TryGetValue(segmentId, out RoadSegment segment))
                        {
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
                        }
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
                    ).ExecuteWith(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if(segments.TryGetValue(segmentId, out RoadSegment segment))
                        {
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
                        }
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
                    ).ExecuteWith(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if(segments.TryGetValue(segmentId, out RoadSegment segment))
                        {
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
                        }
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
                    ).ExecuteWith(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if(segments.TryGetValue(segmentId, out RoadSegment segment))
                        {
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
                        }
                    });
                Console.WriteLine("Enriching segments with surface information ...");                
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
                    ).ExecuteWith(reader =>
                    {
                        var segmentId = reader.GetInt32(0);
                        if(segments.TryGetValue(segmentId, out RoadSegment segment))
                        {
                            var props = new RoadSegmentSurfaceProperties
                            {
                                AttributeId = reader.GetInt32(1),
                                Type = Translate.ToSurfaceType(reader.GetInt32(2)),
                                FromPosition = reader.GetDecimal(3),
                                ToPosition = reader.GetDecimal(4),
                                Origin = new OriginProperties
                                {
                                    OrganizationId = reader.GetNullableString(5),
                                    Organization = reader.GetNullableString(6),
                                    Since = reader.GetDateTime(7)
                                }
                            };
                            var copy = new RoadSegmentSurfaceProperties[segment.Surfaces.Length + 1];
                            segment.Surfaces.CopyTo(copy, 0);
                            copy[segment.Surfaces.Length] = props;
                            segment.Surfaces = copy;
                        }
                    });
                Console.WriteLine("Enriching segments took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                Console.WriteLine("Reading junctions started ...");
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
                    ).ExecuteWith(reader =>
                    {
                        var junction = new GradeSeparatedJunction
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
                connection.Close();
                var after = Process.GetCurrentProcess().VirtualMemorySize64;
                Console.WriteLine("Virtual memory size difference: {0}", after - before);

                var command = new ImportRoadNetwork
                {
                    Nodes = nodes.ToArray(),
                    Segments = segments.Values.ToArray(),
                    GradeSeparatedJunctions = junctions.ToArray(),
                };
                var serializer = JsonSerializer.Create();
                using(var stream = File.OpenWrite("command.json"))
                {
                    using(var writer = new JsonTextWriter(new StreamWriter(stream)))
                    {
                        serializer.Serialize(writer, command);
                        writer.Flush();
                    }
                }
                //AnalyzeNodes(nodes);                
                //AnalyzeSegments(segments.Values.ToList());
            }
        }

        private static void AnalyzeSegments(List<RoadSegment> segments)
        {
            Console.WriteLine("Total # of segments: {0}", segments.Count);
            Console.WriteLine("Segments by status:");
            PrintLines(
                segments
                    .GroupBy(n => n.Status).Select(g => $"{g.Key.ToString()}: {g.Count()}"));
            Console.WriteLine("Segments by morphology:");
            PrintLines(
                segments
                    .GroupBy(n => n.Morphology).Select(g => $"{g.Key.ToString()}: {g.Count()}"));
            Console.WriteLine("Segments by maintainer:");
            PrintLines(
                segments
                    .GroupBy(n => n.MaintainerId).Select(g => $"{g.Key.ToString()}: {g.Count()}"));
            Console.WriteLine("Segments by method:");
            PrintLines(
                segments
                    .GroupBy(n => n.GeometryDrawMethod).Select(g => $"{g.Key.ToString()}: {g.Count()}"));
            Console.WriteLine("Segments by category:");
            PrintLines(
                segments
                    .GroupBy(n => n.Category).Select(g => $"{g.Key.ToString()}: {g.Count()}"));
            Console.WriteLine("Segments by access restriction:");
            PrintLines(
                segments
                    .GroupBy(n => n.AccessRestriction).Select(g => $"{g.Key.ToString()}: {g.Count()}"));
        }

        private static void AnalyzeNodes(List<RoadNode> nodes)
        {
            Console.WriteLine("Total # of nodes: {0}", nodes.Count);
            Console.WriteLine("Nodes by type:");
            PrintLines(
                nodes
                    .GroupBy(n => n.Type)
                    .Select(g => $"{g.Key.ToString()}: {g.Count()}"));
        }

        private static void PrintLines(IEnumerable<string> lines)
        {
            foreach(var line in lines)
            {
                Console.WriteLine("-{0}", line);
            }
        }
    }
}
