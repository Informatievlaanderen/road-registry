namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.FeatureCompare.V3;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V1;
using RoadRegistry.Tests.BackOffice.Extracts.V2;
using Scenarios;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.V3.TranslatedChanges;

public class ExtractRequiredFilesTests : FeatureCompareTranslatorScenariosBase
{
    public ExtractRequiredFilesTests(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task EnsureEachFileIsRequiredOrNot()
    {
        var requiredFiles = new Dictionary<string, bool>
        {
            { "TRANSACTIEZONES.DBF", true },
            { "TRANSACTIEZONES.SHP", false },
            { "TRANSACTIEZONES.PRJ", false },
            { "WEGKNOOP.DBF", true },
            { "EWEGKNOOP.DBF", true },
            { "IWEGKNOOP.DBF", true },
            { "WEGKNOOP.SHP", true },
            { "EWEGKNOOP.SHP", true },
            { "IWEGKNOOP.SHP", true },
            { "WEGKNOOP.PRJ", true },
            { "EWEGKNOOP.PRJ", true },
            { "IWEGKNOOP.PRJ", false },
            { "WEGSEGMENT.DBF", true },
            { "EWEGSEGMENT.DBF", true },
            { "IWEGSEGMENT.DBF", true },
            { "WEGSEGMENT.SHP", true },
            { "EWEGSEGMENT.SHP", true },
            { "IWEGSEGMENT.SHP", true },
            { "WEGSEGMENT.PRJ", true },
            { "EWEGSEGMENT.PRJ", true },
            { "IWEGSEGMENT.PRJ", false },
            { "ATTWEGVERHARDING.DBF", true },
            { "EATTWEGVERHARDING.DBF", true },
            { "ATTEUROPWEG.DBF", true },
            { "EATTEUROPWEG.DBF", true },
            { "ATTNATIONWEG.DBF", true },
            { "EATTNATIONWEG.DBF", true },
            { "RLTOGKRUISING.DBF", true },
            { "ERLTOGKRUISING.DBF", true }
        };

        Assert.Equal(new ExtractV2ZipArchiveTestData().ZipArchiveWithEmptyFiles.Entries.Count, requiredFiles.Count);

        foreach (var requiredFile in requiredFiles)
        {
            var fileName = requiredFile.Key;
            var isRequired = requiredFile.Value;
            TestOutputHelper.WriteLine($"{fileName}: {isRequired}");

            var zipArchive = new ExtractV2ZipArchiveBuilder()
                .ExcludeFileNames(fileName)
                .Build();

            var hasFile = zipArchive.Entries.Any(x => string.Equals(x.Name, fileName, StringComparison.InvariantCultureIgnoreCase));
            Assert.False(hasFile);

            if (isRequired)
            {
                var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
                ex.Problems.Should().ContainSingle(x => x.File == fileName && x.Reason == nameof(ZipArchiveProblems.RequiredFileMissing));
            }
            else
            {
                await TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty);
            }
        }
    }
}
