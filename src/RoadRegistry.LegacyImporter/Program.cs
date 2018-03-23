using System;
using System.Collections.Generic;
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
            var segments = new List<RoadSegment>();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var before = Process.GetCurrentProcess().PrivateMemorySize64;
                Console.WriteLine("Reading nodes started ...");
                var watch = Stopwatch.StartNew();
                using (var command = new SqlCommand(
                    @"SELECT 
                        [wegknoopID],
                        [type],
                        [geometrie]
                    FROM [wegknoop]", connection))
                { // iterate over all nodes
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.IsClosed)
                        {
                            while (await reader.ReadAsync())
                            {
                                var node = new RoadNode
                                {
                                    Id = reader.GetInt32(0),
                                    Type = Translate.ToRoadNodeType(reader.GetInt32(1)),
                                    Geometry = reader.GetAllBytes(2)
                                };
                                nodes.Add(node);
                            }
                        }
                    }
                }
                Console.WriteLine("Reading nodes took {0}ms", watch.ElapsedMilliseconds);
                watch.Restart();
                Console.WriteLine("Reading segments started ...");
                using (var command = new SqlCommand(
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
                        ws.[segmentsStraatnaamID], --10
                        lg.[NISCode], --11
                        ws.[rechtsStraatnaamID], --12
                        rg.[NISCode] --13
                    FROM [dbo].[wegsegment] ws
                    LEFT OUTER JOIN [dbo].[gemeenteNIS] lg ON ws.[segmentsGemeente] = lg.[gemeenteId]
                    LEFT OUTER JOIN [dbo].[gemeenteNIS] rg ON ws.[rechtsGemeente] = rg.[gemeenteId]
                    WHERE ws.[eindWegknoopID] IS NOT NULL", connection))
                { // iterate over all nodes
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!reader.IsClosed)
                        {
                            while (await reader.ReadAsync())
                            {
                                var link = new RoadSegment
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
                                    }
                                };
                                segments.Add(link);
                            }
                        }
                    }
                }
                Console.WriteLine("Reading segments took {0}ms", watch.ElapsedMilliseconds);
                connection.Close();
                var after = Process.GetCurrentProcess().PrivateMemorySize64;
                Console.WriteLine("Private memory size difference: {0}", after - before);
                AnalyzeNodes(nodes);                
                AnalyzeSegments(segments);
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
