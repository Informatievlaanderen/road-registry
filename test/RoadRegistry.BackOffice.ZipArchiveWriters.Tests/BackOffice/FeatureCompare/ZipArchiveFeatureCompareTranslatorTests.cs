namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare
{
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Text;
    using FluentAssertions;
    using MartinCostello.Logging.XUnit;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using RoadRegistry.BackOffice.FeatureCompare;
    using RoadRegistry.BackOffice.FeatureCompare.V1;
    using RoadRegistry.BackOffice.FeatureCompare.V1.Readers;
    using RoadRegistry.BackOffice.Uploads;
    using RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;
    using RoadRegistry.Extensions;
    using RoadRegistry.Extracts;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
    using RoadRegistry.Extracts.Uploads;
    using Xunit.Abstractions;
    using RoadSegmentFeatureCompareFeatureReader = RoadRegistry.BackOffice.FeatureCompare.V1.Readers.RoadSegmentFeatureCompareFeatureReader;

    public class ZipArchiveFeatureCompareTranslatorTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ZipArchiveFeatureCompareTranslatorTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        //[Fact]
        [Fact(Skip = "For debugging purposes, run feature compare v3 on a local archive")]
        public async Task RunFeatureCompareV3()
        {
            var path = @"1acecb107bcc44be91ded9cb7291b9ee.zip";

            var loggerFactory = new LoggerFactory([new XUnitLoggerProvider(_outputHelper, new XUnitLoggerOptions())]);

            var translator = ZipArchiveFeatureCompareTranslatorV3Builder.Create(loggerFactory: loggerFactory,
                grbOgcApiFeaturesDownloader: new GrbOgcApiFeaturesDownloader(new HttpClient(), "https://geo.api.vlaanderen.be/GRB/ogc/features/v1"));

            var sw = Stopwatch.StartNew();
            try
            {
                using (var fileStream = File.OpenRead(path))
                {
                    var archive = new ZipArchive(fileStream);

                    var translatedChanges = await translator.TranslateAsync(archive, ZipArchiveMetadata.Empty.WithInwinning(), CancellationToken.None);
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

        //[Fact]
        [Fact(Skip = "For debugging purposes, run feature compare v2 on a local archive")]
        public async Task RunFeatureCompareV2()
        {
            var path = @"C:\Users\RikDePeuter\Downloads\be5bf26d75e4414fb32243f0565a9cdb.zip";

            var loggerFactory = new LoggerFactory([new XUnitLoggerProvider(_outputHelper, new XUnitLoggerOptions())]);
            var validator = ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create();
            var translator = ZipArchiveFeatureCompareTranslatorV2Builder.Create(loggerFactory: loggerFactory);
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
                    //var sw4 = Stopwatch.StartNew();
                    //for (var i = 0; i < 10; i++)
                    //{
                    //    var sw3 = Stopwatch.StartNew();
                    //    await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);
                    //    _outputHelper.WriteLine($"Duration validator run {i}: {sw3.Elapsed}");
                    //}
                    //var elapsed4 = sw4.Elapsed;
                    //_outputHelper.WriteLine($"Total duration validator 10 runs: {sw4.Elapsed}");

                    await translator.TranslateAsync(archive, CancellationToken.None);
                    // var sw2 = Stopwatch.StartNew();
                    // for (var i = 0; i < 10; i++)
                    // {
                    //     var sw3 = Stopwatch.StartNew();
                    //     var changes = await translator.TranslateAsync(archive, CancellationToken.None);
                    //     _outputHelper.WriteLine($"Duration translator run {i}: {sw3.Elapsed}");
                    // }

                    //var elapsed = sw2.Elapsed;
                    //_outputHelper.WriteLine($"Total duration translator 10 runs: {sw2.Elapsed}");
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

        private Dictionary<ExtractFileName, int> ReadV1(ZipArchive archive)
        {
            var encoding = FileEncoding.UTF8;

            var transactionZones = new TransactionZoneFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.Transactiezones, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var nodes = new RoadNodeFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.Wegknoop, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var segments = new RoadSegmentFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.Wegsegment, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var lanes = new RoadSegmentLaneFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttRijstroken, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var widths = new RoadSegmentWidthFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttWegbreedte, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var surfaces = new RoadSegmentSurfaceFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttWegverharding, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var europeanRoads = new EuropeanRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttEuropweg, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var nationalRoads = new NationalRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttNationweg, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var numberedRoads = new NumberedRoadFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.AttGenumweg, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));
            var junctions = new GradeSeparatedJunctionFeatureCompareFeatureReader(encoding)
                .Read(archive, FeatureType.Extract, ExtractFileName.RltOgkruising, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

            return new Dictionary<ExtractFileName, int>
            {
                { ExtractFileName.Transactiezones, transactionZones.Item1.Count },
                { ExtractFileName.Wegknoop, nodes.Item1.Count },
                { ExtractFileName.Wegsegment, segments.Item1.Count },
                { ExtractFileName.AttRijstroken, lanes.Item1.Count },
                { ExtractFileName.AttWegbreedte, widths.Item1.Count },
                { ExtractFileName.AttWegverharding, surfaces.Item1.Count },
                { ExtractFileName.AttEuropweg, europeanRoads.Item1.Count },
                { ExtractFileName.AttNationweg, nationalRoads.Item1.Count },
                { ExtractFileName.AttGenumweg, numberedRoads.Item1.Count },
                { ExtractFileName.RltOgkruising, junctions.Item1.Count }
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
                { ExtractFileName.Transactiezones, transactionZones.Item1.Count },
                { ExtractFileName.Wegknoop, nodes.Item1.Count },
                { ExtractFileName.Wegsegment, segments.Item1.Count },
                { ExtractFileName.AttRijstroken, lanes.Item1.Count },
                { ExtractFileName.AttWegbreedte, widths.Item1.Count },
                { ExtractFileName.AttWegverharding, surfaces.Item1.Count },
                { ExtractFileName.AttEuropweg, europeanRoads.Item1.Count },
                { ExtractFileName.AttNationweg, nationalRoads.Item1.Count },
                { ExtractFileName.AttGenumweg, numberedRoads.Item1.Count },
                { ExtractFileName.RltOgkruising, junctions.Item1.Count }
            };
        }

        private static MultiPolygon BuildTransactionZoneGeometry(int x, int y, int width)
        {
            var wkt = $"MULTIPOLYGON ((({x} {y}, {x} {y + width}, {x + width} {y + width}, {x + width} {y}, {x} {y})))";
            var geometry = new WKTReader().Read(wkt);
            geometry.SRID = 31370;
            return geometry.ToMultiPolygon();
        }

        //[Fact]
        [Fact(Skip = "For debugging purposes, test GRB uploads with V1 and V2 readers")]
        public async Task TestGrbUploads()
        {
            var archiveDirectory = @"C:\Users\RikDePeuter\Downloads";
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

            var versions = new[] { WellKnownZipArchiveWriterVersions.DomainV1_1, WellKnownZipArchiveWriterVersions.DomainV1_2 };
            var validators = new Dictionary<string, IZipArchiveBeforeFeatureCompareValidator>
            {
                { WellKnownZipArchiveWriterVersions.DomainV1_1, ZipArchiveBeforeFeatureCompareValidatorV1Builder.Create() },
                { WellKnownZipArchiveWriterVersions.DomainV1_2, ZipArchiveBeforeFeatureCompareValidatorV2Builder.Create() }
            };
            var translators = new Dictionary<string, IZipArchiveFeatureCompareTranslator>
            {
                { WellKnownZipArchiveWriterVersions.DomainV1_1, ZipArchiveFeatureCompareTranslatorV1Builder.Create() },
                { WellKnownZipArchiveWriterVersions.DomainV1_2, ZipArchiveFeatureCompareTranslatorV2Builder.Create() }
            };
            var readers = new Dictionary<string, Func<ZipArchive, Dictionary<ExtractFileName, int>>>
            {
                { WellKnownZipArchiveWriterVersions.DomainV1_1, ReadV1 },
                { WellKnownZipArchiveWriterVersions.DomainV1_2, ReadV2 }
            };

            foreach (var archiveId in archiveIds)
            {
                var sw = Stopwatch.StartNew();
                _outputHelper.WriteLine($"Archive: {archiveId}");

                var readResults = new Dictionary<string, Dictionary<ExtractFileName, int>>();
                var validationResults = new Dictionary<string, ZipArchiveProblems>();
                var changes = new Dictionary<string, TranslatedChanges>();

                foreach (var zipArchiveWriterVersion in versions)
                {
                    sw.Restart();
                    try
                    {
                        var validator = validators[zipArchiveWriterVersion];
                        var translator = translators[zipArchiveWriterVersion];

                        await using var stream = File.OpenRead($@"{archiveDirectory}\{archiveId}.zip");
                        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true, Encoding.UTF8);
                        await validator.ValidateAsync(archive, ZipArchiveMetadata.Empty, CancellationToken.None);
                        validationResults[zipArchiveWriterVersion] = ZipArchiveProblems.None;

                        readResults[zipArchiveWriterVersion] = readers[zipArchiveWriterVersion](archive);

                        changes[zipArchiveWriterVersion] = await translator.TranslateAsync(archive, CancellationToken.None);
                    }
                    catch (ZipArchiveValidationException ex)
                    {
                        validationResults[zipArchiveWriterVersion] = ex.Problems;
                        changes[zipArchiveWriterVersion] = TranslatedChanges.Empty;
                        continue;
                    }

                    _outputHelper.WriteLine($"{zipArchiveWriterVersion} FeatureCompare: {sw.Elapsed}");
                }

                var v1Result = readResults[WellKnownZipArchiveWriterVersions.DomainV1_1];
                var v2Result = readResults[WellKnownZipArchiveWriterVersions.DomainV1_2];

                foreach (var v1 in v1Result)
                {
                    v2Result[v1.Key].Should().Be(v1.Value);
                }

                var v1ValidationResult = validationResults[WellKnownZipArchiveWriterVersions.DomainV1_1];
                var v2ValidationResult = validationResults[WellKnownZipArchiveWriterVersions.DomainV1_2];
                v2ValidationResult.Should().BeEquivalentTo(v1ValidationResult);

                var v1Changes = changes[WellKnownZipArchiveWriterVersions.DomainV1_1];
                var v2Changes = changes[WellKnownZipArchiveWriterVersions.DomainV1_2];
                v2Changes.Count.Should().Be(v1Changes.Count);

                _outputHelper.WriteLine(string.Empty);
            }
        }
    }
}
