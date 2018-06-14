namespace RoadRegistry.LegacyStreamExtraction
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
    using Shaperon;

    public class Program
    {
        private static async Task Main(string[] args)
        {
            const string LEGACY_STREAM_FILE = "LegacyStreamFile";

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var root = configurationBuilder.Build();
            var output = new FileInfo(root[LEGACY_STREAM_FILE]);
            var connectionString = root.GetConnectionString("Legacy");
            var nodes = new List<ImportedRoadNode>();
            var points = new List<ImportedReferencePoint>();
            var segments = new Dictionary<int, ImportedRoadSegment>();
            var junctions = new List<ImportedGradeSeparatedJunction>();
            var organizations = new List<ImportedOrganization>();

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
                        organizations.Add(organization);
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
                        // var gem = Wkx.Geometry.Deserialize<Wkx.WkbSerializer>(bytes);
                        // Console.WriteLine(gem.GeometryType);
                        var node = new ImportedRoadNode
                        {
                            Id = reader.GetInt32(0),
                            Version = reader.GetInt32(1),
                            Type = Translate.ToRoadNodeType(reader.GetInt32(2)),
                            Geometry = new Geometry
                            {
                                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972,
                                WellKnownBinary = wellKnownBinary
                            },
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(4),
                                Organization = reader.GetNullableString(5),
                                Since = reader.GetDateTime(6)
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
                            ws.[wegsegmentversie], --1
                            ws.[beginWegknoopID], --2
                            ws.[eindWegknoopID], --3
                            ws.[geometrie].AsBinaryZM(), --4
                            ws.[geometrieversie], --5
                            ws.[beheerder], --6
                            ws.[methode], --7
                            ws.[morfologie], --8
                            ws.[status], --9
                            ws.[categorie], --10
                            ws.[toegangsbeperking], --11
                            ws.[linksStraatnaamID], --12
                            ls.[LOS], --13
                            lg.[NISCode], --14
                            lg.[naam], --15
                            ws.[rechtsStraatnaamID], --16
                            rs.[LOS], --17
                            rg.[NISCode], --18
                            rg.[naam], --19
                            ws.[opnamedatum], --20
                            ws.[beginorganisatie], --21
                            lo.[label], --22
                            ws.[begintijd], --23
                            beheerders.[label] --24
                        FROM [dbo].[wegsegment] ws
                        LEFT OUTER JOIN [dbo].[gemeenteNIS] lg ON ws.[linksGemeente] = lg.[gemeenteId]
                        LEFT OUTER JOIN [dbo].[crabsnm] ls ON ws.[linksStraatnaamID] = ls.[EXN]
                        LEFT OUTER JOIN [dbo].[gemeenteNIS] rg ON ws.[rechtsGemeente] = rg.[gemeenteId]
                        LEFT OUTER JOIN [dbo].[crabsnm] rs ON ws.[rechtsStraatnaamID] = rs.[EXN]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON ws.[beginorganisatie] = lo.[code]
                        LEFT OUTER JOIN [dbo].[listOrganisatie] beheerders ON ws.[beheerder] = beheerders.[code]
                        WHERE ws.[eindWegknoopID] IS NOT NULL", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var wellKnownBinary = reader.GetAllBytes(4);
                        var segment = new ImportedRoadSegment
                        {
                            Id = reader.GetInt32(0),
                            Version = reader.GetInt32(1),
                            StartNodeId = reader.GetInt32(2),
                            EndNodeId = reader.GetInt32(3),
                            Geometry = new VersionedGeometry
                            {
                                Version = reader.GetInt32(5),
                                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972,
                                WellKnownBinary = wellKnownBinary
                            },
                            Maintainer = new Maintainer
                            {
                                Code = reader.GetString(6),
                                Name = reader.GetString(24)
                            },
                            GeometryDrawMethod = Translate.ToRoadSegmentGeometryDrawMethod(reader.GetInt32(7)),
                            Morphology = Translate.ToRoadSegmentMorphology(reader.GetInt32(8)),
                            Status = Translate.ToRoadSegmentStatus(reader.GetInt32(9)),
                            Category = Translate.ToRoadSegmentCategory(reader.GetString(10)),
                            AccessRestriction = Translate.ToRoadSegmentAccessRestriction(reader.GetInt32(11)),
                            LeftSide = new RoadSegmentSideProperties
                            {
                                StreetNameId = reader.GetNullableInt32(12),
                                StreetName = reader.GetNullableString(13),
                                MunicipalityNISCode = reader.GetNullableString(14),
                                Municipality = reader.GetNullableString(15)
                            },
                            RightSide = new RoadSegmentSideProperties
                            {
                                StreetNameId = reader.GetNullableInt32(16),
                                StreetName = reader.GetNullableString(17),
                                MunicipalityNISCode = reader.GetNullableString(18),
                                Municipality = reader.GetNullableString(19)
                            },
                            RecordingDate = reader.GetDateTime(20),
                            Origin = new OriginProperties
                            {
                                OrganizationId = reader.GetNullableString(21),
                                Organization = reader.GetNullableString(22),
                                Since = reader.GetDateTime(23)
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
                            ,rp.[opschrift] --4
                            ,rp.[beginorganisatie] --5
                            ,lo.[label] --6
                            ,rp.[begintijd] --7
                            ,rp.[referentiepuntversie] --8
                        FROM [dbo].[referentiepunt] rp
                        LEFT OUTER JOIN [dbo].[listOrganisatie] lo ON rp.[beginorganisatie] = lo.[code]", connection
                    ).ForEachDataRecord(reader =>
                    {
                        var point = new ImportedReferencePoint
                        {
                            Id = reader.GetInt32(0),
                            Version = reader.GetInt32(8),
                            Geometry = new Geometry
                            {
                                SpatialReferenceSystemIdentifier = SpatialReferenceSystemIdentifier.BelgeLambert1972,
                                WellKnownBinary = reader.GetAllBytes(1)
                            },
                            Ident8 = reader.GetString(2),
                            Type = Translate.ToReferencePointType(reader.GetInt32(3)),
                            Caption = (double)reader.GetDecimal(4),
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
                await new ExtractedStreamsWriter(output)
                    .WriteAsync(organizations, nodes, segments.Values, junctions, points);
                Console.WriteLine("Writing stream to json took {0}ms", watch.ElapsedMilliseconds);
            }
        }
    }
}
