namespace RoadRegistry.Tests.BackOffice.Uploads
{
    using Microsoft.Extensions.Logging;
    using RoadRegistry.BackOffice.FeatureCompare;
    using RoadRegistry.BackOffice.Uploads;
    using System;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Exceptions;
    using RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

    public class FeatureCompareZipArchiveTranslatorTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly ZipArchiveFeatureCompareTranslator _sut;
        private readonly ZipArchiveTranslator _zipArchiveTranslator;

        public FeatureCompareZipArchiveTranslatorTests(ITestOutputHelper outputHelper, ILogger<FeatureCompareZipArchiveTranslatorTests> logger)
        {
            _outputHelper = outputHelper;
            _sut = new ZipArchiveFeatureCompareTranslator(Encoding.UTF8, logger);
            _zipArchiveTranslator = new ZipArchiveTranslator(Encoding.UTF8);
        }

        [Fact]
        public void ArchiveCanNotBeNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sut.Translate(null, CancellationToken.None));
        }

        [Theory(Skip = "For local testing only due to big archive files")]
        //[Theory]
        //[InlineData("Oudenburg")]
        //[InlineData("Dendermonde")]
        //[InlineData("Aarschot")]
        //[InlineData("Tervuren")]
        //[InlineData("Antwerpen")]
        [InlineData("Gent")]
        public async Task TranslateWithRecordsReturnsExpectedResult(string zipFileName)
        {
            using (var beforeFcArchiveStream = new MemoryStream())
            using (var afterFcArchiveStream = new MemoryStream())
            {
                await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.BackOffice.Uploads.FeatureCompareZipArchiveTranslatorData.{zipFileName}-before.zip"))
                {
                    await embeddedStream!.CopyToAsync(beforeFcArchiveStream);
                }
                beforeFcArchiveStream.Position = 0;

                await using (var embeddedStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"RoadRegistry.Tests.BackOffice.Uploads.FeatureCompareZipArchiveTranslatorData.{zipFileName}-after.zip"))
                {
                    await embeddedStream!.CopyToAsync(afterFcArchiveStream);
                }
                afterFcArchiveStream.Position = 0;

                using (var beforeFcArchive = new ZipArchive(beforeFcArchiveStream))
                using (var afterFcArchive = new ZipArchive(afterFcArchiveStream))
                {
                    //var validator = new ZipArchiveBeforeFeatureCompareValidator(FileEncoding.UTF8);
                    //var validationResult = validator.Validate(beforeFcArchive, ZipArchiveMetadata.Empty);
                    //var fileErrors = validationResult.OfType<FileError>().ToArray();
                    //if (fileErrors.Any())
                    //{
                    //    foreach (var problem in fileErrors)
                    //    {
                    //        _outputHelper.WriteLine($"{problem.File}: {problem.Reason}, Parameters: {string.Join(", ", problem.Parameters.Select(p => $"{p.Name}={p.Value}"))}");
                    //    }
                    //    throw new ZipArchiveValidationException(validationResult);
                    //}

                    var sw = Stopwatch.StartNew();
                    _outputHelper.WriteLine($"{zipFileName} started translate Before-FC");
                    var changes = await _sut.Translate(beforeFcArchive, CancellationToken.None);
                    _outputHelper.WriteLine($"{zipFileName} finished translate Before-FC at {sw.Elapsed}");

                    sw = Stopwatch.StartNew();
                    _outputHelper.WriteLine($"{zipFileName} started translate After-FC");
                    var expected = _zipArchiveTranslator.Translate(afterFcArchive);
                    _outputHelper.WriteLine($"{zipFileName} finished translate After-FC at {sw.Elapsed}");

                    Assert.Equal(expected, changes, new TranslatedChangeEqualityComparer(ignoreRecordNumber: true));
                }
            }
        }
    }
}
