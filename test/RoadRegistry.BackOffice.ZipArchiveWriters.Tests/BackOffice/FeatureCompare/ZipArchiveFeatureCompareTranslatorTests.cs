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

            var encoding = Encoding.UTF8;
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

            var sw = Stopwatch.StartNew();

            foreach (var transactionZoneGeometry in transactionZoneGeometries)
            {
                _outputHelper.WriteLine($"Transaction Zone: {transactionZoneGeometry}");

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

                        var changes = await translator.TranslateAsync(archive, CancellationToken.None);

                        //TODO-pr hoe resultaat vergelijken? de readers gebruiken?
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
            }
        }

        private static MultiPolygon BuildTransactionZoneGeometry(int x, int y, int width)
        {
            var wkt = $"MULTIPOLYGON ((({x} {y}, {x} {y+width}, {x+width} {y+width}, {x+width} {y}, {x} {y})))";
            var geometry = new WKTReader().Read(wkt);
            geometry.SRID = 31370;
            return geometry.ToMultiPolygon();
        }
    }
}
