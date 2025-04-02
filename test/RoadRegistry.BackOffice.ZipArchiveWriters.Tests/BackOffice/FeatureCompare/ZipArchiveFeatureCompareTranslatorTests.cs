namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare
{
    using System.IO.Compression;
    using System.Text;
    using Exceptions;
    using ExtractHost.V2;
    using Extracts;
    using Extracts.Dbase;
    using NetTopologySuite.IO;
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
                await writer.WriteToArchive(archive, ExtractFileName.Transactiezones, FeatureType.Change, TransactionZoneDbaseRecord.Schema, [
                    (dbfRecord, new WKTReader().Read("MULTIPOLYGON(((55000 200000,55000 200100,55100 200100,55100 200000,55000 200000)))"))
                ], CancellationToken.None);
            }

            archiveStream.Position = 0;
            await File.WriteAllBytesAsync("output.zip", archiveStream.ToArray());
        }

        //[Fact]
        [Fact(Skip = "For debugging purposes, run feature compare on a local archive")]
        public async Task RunFeatureCompare()
        {
            var path = @"db34013eebf456f87da95b6c34c63c9.zip";

            var validator = ZipArchiveBeforeFeatureCompareValidatorBuilder.Create();

            var translator = ZipArchiveFeatureCompareTranslatorBuilder.Create();
            /*
            var translator = ZipArchiveFeatureCompareTranslator.Create([
                new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(FileEncoding.UTF8)),
                new RoadSegmentFeatureCompareTranslator(new RoadSegmentFeatureCompareFeatureReader(FileEncoding.UTF8), new FakeOrganizationCache(), new FakeStreetNameCache()),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(FileEncoding.UTF8)),
            ], new NullLogger<ZipArchiveFeatureCompareTranslator>());
            */
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
        }
    }
}
