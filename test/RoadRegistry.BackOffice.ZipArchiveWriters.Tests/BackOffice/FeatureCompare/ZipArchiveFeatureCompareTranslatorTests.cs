namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare
{
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Text;
    using Abstractions;
    using AutoFixture;
    using Editor.Schema;
    using Exceptions;
    using ExtractHost;
    using ExtractHost.V2;
    using Extracts;
    using Extracts.Dbase;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.IO.Esri;
    using RoadRegistry.BackOffice.FeatureCompare;
    using RoadRegistry.Tests.BackOffice.Scenarios;
    using Uploads;
    using Xunit.Abstractions;

    public class ZipArchiveFeatureCompareTranslatorTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ZipArchiveFeatureCompareTranslatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        //TODO-pr create unit tests, this was poc
        [Fact]
        public async Task TestDbaseRecordWriter()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var dbfRecord = new TransactionZoneDbaseRecord
            {
                SOURCEID = { Value = 1 },
                TYPE = { Value = 2 },
                BESCHRIJV =
                {
                    Value = "abc"
                },
                OPERATOR = { Value = "" },
                ORG = { Value = "AGIV" },
                APPLICATIE = { Value = "Wegenregister" }
            };

            using var archiveStream = new MemoryStream();
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
            {
                var writer = new DbaseRecordWriter(WellKnownEncodings.WindowsAnsi);
                await writer.WriteToArchive(
                    archive,
                    ExtractFileName.Transactiezones,
                    FeatureType.Change,
                    TransactionZoneDbaseRecord.Schema,
                    ShapeType.Polygon,
                    [
                        (dbfRecord, new WKTReader().Read("MULTIPOLYGON(((55000 200000,55000 200100,55100 200100,55100 200000,55000 200000)))"))
                    ],
                    CancellationToken.None);
            }

            archiveStream.Position = 0;
            await File.WriteAllBytesAsync("output.zip", archiveStream.ToArray());
        }

        [Fact]
        //[Fact(Skip = "For debugging purposes, run feature compare on a local archive")]
        public async Task RunFeatureCompareV1()
        {
            var path = @"C:\Users\RikDePeuter\Downloads\portaal_upload.zip";

            var validator = ZipArchiveBeforeFeatureCompareValidatorV1Builder.Create();
            var translator = ZipArchiveFeatureCompareTranslatorV1Builder.Create();
            /*
            var translator = ZipArchiveFeatureCompareTranslator.Create([
                new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(FileEncoding.UTF8)),
                new RoadSegmentFeatureCompareTranslator(new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8), new FakeOrganizationCache(), new FakeStreetNameCache()),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(FileEncoding.UTF8)),
            ], new NullLogger<ZipArchiveFeatureCompareTranslator>());
            */
            var sw = Stopwatch.StartNew();
            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var archive = new ZipArchive(fileStream);
                    await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);

                    var changes = await translator.TranslateAsync(archive, CancellationToken.None);
                }
            }
            catch (ZipArchiveValidationException ex)
            {
                foreach (var problem in ex.Problems)
                {
                    _outputHelper.WriteLine(problem.Describe());
                }

                throw;
            }

            _outputHelper.WriteLine($"Duration: {sw.Elapsed}");
        }

        [Fact]
        //[Fact(Skip = "For debugging purposes, run feature compare on a local archive")]
        public async Task RunFeatureCompareV2()
        {
            var path = @"C:\Users\RikDePeuter\Downloads\portaal_upload.zip";

            var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create();
            var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create();
            /*
            var translator = ZipArchiveFeatureCompareTranslator.Create([
                new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(FileEncoding.UTF8)),
                new RoadSegmentFeatureCompareTranslator(new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8), new FakeOrganizationCache(), new FakeStreetNameCache()),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(FileEncoding.UTF8)),
            ], new NullLogger<ZipArchiveFeatureCompareTranslator>());
            */
            var sw = Stopwatch.StartNew();
            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var archive = new ZipArchive(fileStream);
                    await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);

                    var changes = await translator.TranslateAsync(archive, CancellationToken.None);
                }
            }
            catch (ZipArchiveValidationException ex)
            {
                foreach (var problem in ex.Problems)
                {
                    _outputHelper.WriteLine(problem.Describe());
                }

                throw;
            }

            _outputHelper.WriteLine($"Duration: {sw.Elapsed}");
        }

        [Fact]
        public async Task CompareExtractV1AndV2()
        {
            var transactionZoneGeometries = new List<MultiPolygon>
                {
                    BuildTransactionZoneGeometry(232300, 165000, 10000),
                };

            var connectionString = "Data Source=tcp:localhost,21433;Initial Catalog=road-registry;Integrated Security=False;User ID=sa;Password=E@syP@ssw0rd;TrustServerCertificate=True";

            var encoding = FileEncoding.UTF8;
            var manager = new RecyclableMemoryStreamManager();
            var fixture = new RoadNetworkTestData().ObjectProvider;

            var writerV1 = new ExtractHost.V1.RoadNetworkExtractToZipArchiveWriter(
                new ZipArchiveWriterOptions(),
                new FakeStreetNameCache(),
                manager,
                encoding,
                new NullLoggerFactory()
            );
            var writerV2 = new ExtractHost.V2.RoadNetworkExtractZipArchiveWriter(
                new ZipArchiveWriterOptions(),
                new FakeStreetNameCache(),
                manager,
                encoding,
                new NullLoggerFactory()
            );

            var writers = new Dictionary<string, IZipArchiveWriter>
            {
                { WellKnownZipArchiveWriterVersions.V1, writerV1 },
                { WellKnownZipArchiveWriterVersions.V2, writerV2 }
            };
            var validators = new Dictionary<string, IZipArchiveBeforeFeatureCompareValidator>
            {
                { WellKnownZipArchiveWriterVersions.V1, ZipArchiveBeforeFeatureCompareValidatorV1Builder.Create() },
                { WellKnownZipArchiveWriterVersions.V2, ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create() }
            };
            var translators = new Dictionary<string, IZipArchiveFeatureCompareTranslator>
            {
                { WellKnownZipArchiveWriterVersions.V1, ZipArchiveFeatureCompareTranslatorV1Builder.Create() },
                { WellKnownZipArchiveWriterVersions.V2, ZipArchiveFeatureCompareTranslatorV2Builder.Create() }
            };
            var readers = new Dictionary<string, Func<ZipArchive, Dictionary<ExtractFileName, int>>>
            {
                { WellKnownZipArchiveWriterVersions.V1, ReadV1 },
                { WellKnownZipArchiveWriterVersions.V2, ReadV2 }
            };

            var sw = Stopwatch.StartNew();

            foreach (var transactionZoneGeometry in transactionZoneGeometries)
            {
                _outputHelper.WriteLine($"Transaction Zone: {transactionZoneGeometry}");

                var readResults = new Dictionary<string, Dictionary<ExtractFileName, int>>();

                foreach (var writer in writers)
                {
                    var zipArchiveWriterVersion = writer.Key;

                    var request = new RoadNetworkExtractAssemblyRequest(
                        fixture.Create<ExternalExtractRequestId>(),
                        fixture.Create<DownloadId>(),
                        fixture.Create<ExtractDescription>(),
                        transactionZoneGeometry,
                        isInformative: false);

                    await using var stream = manager.GetStream();
                    await using var context = new EditorContext(new DbContextOptionsBuilder<EditorContext>()
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .UseSqlServer(connectionString, options =>
                            options.UseNetTopologySuite()
                        ).Options);

                    {
                        //warmup db
                        {
                            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
                            await writer.Value.WriteAsync(archive, request, new ZipArchiveDataProvider(context), CancellationToken.None);
                        }

                        //actual run
                        {
                            sw.Restart();
                            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
                            await writer.Value.WriteAsync(archive, request, new ZipArchiveDataProvider(context), CancellationToken.None);
                            _outputHelper.WriteLine($"{zipArchiveWriterVersion} ZArchiveWriter: {sw.Elapsed}");
                        }
                    }

                    sw.Restart();
                    try
                    {
                        var validator = validators[zipArchiveWriterVersion];
                        var translator = translators[zipArchiveWriterVersion];

                        stream.Position = 0;
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true, Encoding.UTF8);
                        await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);

                        readResults[zipArchiveWriterVersion] = readers[zipArchiveWriterVersion](archive);

                        var changes = await translator.TranslateAsync(archive, CancellationToken.None);
                    }
                    catch (ZipArchiveValidationException ex)
                    {
                        _outputHelper.WriteLine($"{zipArchiveWriterVersion} FeatureCompare validation failed:");
                        foreach (var problem in ex.Problems)
                        {
                            _outputHelper.WriteLine($"  {problem.Describe()}");
                        }
                    }

                    _outputHelper.WriteLine($"{zipArchiveWriterVersion} FeatureCompare: {sw.Elapsed}");
                }

                var v1Result = readResults[WellKnownZipArchiveWriterVersions.V1];
                var v2Result = readResults[WellKnownZipArchiveWriterVersions.V2];

                foreach (var v1 in v1Result)
                {
                    v2Result[v1.Key].Should().Be(v1.Value);
                }

                _outputHelper.WriteLine(string.Empty);
            }
        }

        private Dictionary<ExtractFileName, int> ReadV1(ZipArchive archive)
        {
            var encoding = FileEncoding.UTF8;

            var transactionZones = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.TransactionZoneFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.Transactiezones, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var nodes = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.RoadNodeFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.Wegknoop, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var segments = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.RoadSegmentFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.Wegsegment, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var lanes = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.RoadSegmentLaneFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttRijstroken, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var widths = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.RoadSegmentWidthFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttWegbreedte, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var surfaces = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.RoadSegmentSurfaceFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttWegverharding, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var europeanRoads = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.EuropeanRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttEuropweg, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var nationalRoads = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.NationalRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttNationweg, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var numberedRoads = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.NumberedRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttGenumweg, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var junctions = new RoadRegistry.BackOffice.FeatureCompare.V1.Readers.GradeSeparatedJunctionFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.RltOgkruising, new RoadRegistry.BackOffice.FeatureCompare.V1.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

            return new Dictionary<ExtractFileName, int>
            {
                { ExtractFileName.Transactiezones, transactionZones.Item1.Count},
                { ExtractFileName.Wegknoop, nodes.Item1.Count},
                { ExtractFileName.Wegsegment, segments.Item1.Count},
                { ExtractFileName.AttRijstroken, lanes.Item1.Count},
                { ExtractFileName.AttWegbreedte, widths.Item1.Count},
                { ExtractFileName.AttWegverharding, surfaces.Item1.Count},
                { ExtractFileName.AttEuropweg, europeanRoads.Item1.Count},
                { ExtractFileName.AttNationweg, nationalRoads.Item1.Count},
                { ExtractFileName.AttGenumweg, numberedRoads.Item1.Count},
                { ExtractFileName.RltOgkruising, junctions.Item1.Count}
            };
        }

        private Dictionary<ExtractFileName, int> ReadV2(ZipArchive archive)
        {
            var encoding = FileEncoding.UTF8;

            var transactionZones = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.TransactionZoneFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var nodes = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.RoadNodeFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var segments = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.RoadSegmentFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var lanes = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.RoadSegmentLaneFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var widths = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.RoadSegmentWidthFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var surfaces = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.RoadSegmentSurfaceFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var europeanRoads = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.EuropeanRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var nationalRoads = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.NationalRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var numberedRoads = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.NumberedRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var junctions = new RoadRegistry.BackOffice.FeatureCompare.V2.Readers.GradeSeparatedJunctionFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, new RoadRegistry.BackOffice.FeatureCompare.V2.ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

            return new Dictionary<ExtractFileName, int>
            {
                { ExtractFileName.Transactiezones, transactionZones.Item1.Count},
                { ExtractFileName.Wegknoop, nodes.Item1.Count},
                { ExtractFileName.Wegsegment, segments.Item1.Count},
                { ExtractFileName.AttRijstroken, lanes.Item1.Count},
                { ExtractFileName.AttWegbreedte, widths.Item1.Count},
                { ExtractFileName.AttWegverharding, surfaces.Item1.Count},
                { ExtractFileName.AttEuropweg, europeanRoads.Item1.Count},
                { ExtractFileName.AttNationweg, nationalRoads.Item1.Count},
                { ExtractFileName.AttGenumweg, numberedRoads.Item1.Count},
                { ExtractFileName.RltOgkruising, junctions.Item1.Count}
            };
        }

        private static MultiPolygon BuildTransactionZoneGeometry(int x, int y, int width)
        {
            var wkt = $"MULTIPOLYGON ((({x} {y}, {x} {y+width}, {x+width} {y+width}, {x+width} {y}, {x} {y})))";
            var geometry = new WKTReader().Read(wkt);
            geometry.SRID = 31370;
            return geometry.ToMultiPolygon();
        }

        //TODO-pr maandag: schema validatie in v2 bij grb upload failed, te bekijken
        [Fact]
        public async Task TestGrbUploads()
        {
            var archiveIds = new[]
            {
                "7034c4b386c64e3aba0602a03d53b6b6",
                "ddd9e8dc570e4f8e89c6c5a3843e0f8e",
                "2e7942d2dd4244aea3e691301f28ea41",
                "fdcfb3652d6248628ef29c686f77c140",
                "daa3da2ffe2f4ae9a469806018a2931e",
                "4f19090f16e2479087bc1666e16d48ed",
                "fe6a00a25c6c4ccaa298522de36ad462",
                "ce7ee4430e4b49c18055301fd81e0117",
                "31622e7ff8644788b6a8944320bd1b84",
                "550102c0d47047418501628b5aaff090"
            };

            var versions = new[] { WellKnownZipArchiveWriterVersions.V1, WellKnownZipArchiveWriterVersions.V2 };
            var validators = new Dictionary<string, IZipArchiveBeforeFeatureCompareValidator>
            {
                { WellKnownZipArchiveWriterVersions.V1, ZipArchiveBeforeFeatureCompareValidatorV1Builder.Create() },
                { WellKnownZipArchiveWriterVersions.V2, ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create() }
            };
            var translators = new Dictionary<string, IZipArchiveFeatureCompareTranslator>
            {
                { WellKnownZipArchiveWriterVersions.V1, ZipArchiveFeatureCompareTranslatorV1Builder.Create() },
                { WellKnownZipArchiveWriterVersions.V2, ZipArchiveFeatureCompareTranslatorV2Builder.Create() }
            };
            var readers = new Dictionary<string, Func<ZipArchive, Dictionary<ExtractFileName, int>>>
            {
                { WellKnownZipArchiveWriterVersions.V1, ReadV1 },
                { WellKnownZipArchiveWriterVersions.V2, ReadV2 }
            };

            foreach (var archiveId in archiveIds)
            {
                var sw = Stopwatch.StartNew();
                _outputHelper.WriteLine($"Archive: {archiveId}");

                var readResults = new Dictionary<string, Dictionary<ExtractFileName, int>>();

                foreach (var zipArchiveWriterVersion in versions)
                {
                    sw.Restart();
                    try
                    {
                        var validator = validators[zipArchiveWriterVersion];
                        var translator = translators[zipArchiveWriterVersion];

                        await using var stream = File.OpenRead($@"C:\Users\RikDePeuter\Downloads\{archiveId}.zip");
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true, Encoding.UTF8);
                        await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);

                        readResults[zipArchiveWriterVersion] = readers[zipArchiveWriterVersion](archive);

                        var changes = await translator.TranslateAsync(archive, CancellationToken.None);
                    }
                    catch (ZipArchiveValidationException ex)
                    {
                        _outputHelper.WriteLine($"{zipArchiveWriterVersion} FeatureCompare validation failed:");
                        foreach (var problem in ex.Problems)
                        {
                            _outputHelper.WriteLine($"  {problem.Describe()}");
                        }
                        throw;
                    }

                    _outputHelper.WriteLine($"{zipArchiveWriterVersion} FeatureCompare: {sw.Elapsed}");
                }

                var v1Result = readResults[WellKnownZipArchiveWriterVersions.V1];
                var v2Result = readResults[WellKnownZipArchiveWriterVersions.V2];

                foreach (var v1 in v1Result)
                {
                    v2Result[v1.Key].Should().Be(v1.Value);
                }

                _outputHelper.WriteLine(string.Empty);
            }
        }
    }
}
