namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Scenarios;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

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
            { "TRANSACTIEZONES.SHP", true },
            { "TRANSACTIEZONES.PRJ", true },
            { "WEGKNOOP.DBF", true },
            { "EWEGKNOOP.DBF", true },
            { "IWEGKNOOP.DBF", true },
            { "WEGKNOOP.SHP", true },
            { "EWEGKNOOP.SHP", true },
            { "IWEGKNOOP.SHP", true },
            { "WEGKNOOP.PRJ", true },
            { "EWEGKNOOP.PRJ", true },
            { "IWEGKNOOP.PRJ", true },
            { "WEGSEGMENT.DBF", true },
            { "EWEGSEGMENT.DBF", true },
            { "IWEGSEGMENT.DBF", true },
            { "WEGSEGMENT.SHP", true },
            { "EWEGSEGMENT.SHP", true },
            { "IWEGSEGMENT.SHP", true },
            { "WEGSEGMENT.PRJ", true },
            { "EWEGSEGMENT.PRJ", true },
            { "IWEGSEGMENT.PRJ", true },
            { "ATTEUROPWEG.DBF", true },
            { "EATTEUROPWEG.DBF", true },
            { "ATTNATIONWEG.DBF", true },
            { "EATTNATIONWEG.DBF", true },
            { "RLTOGKRUISING.DBF", true },
            { "ERLTOGKRUISING.DBF", true }
        };

        Assert.Equal(new DomainV2ZipArchiveTestData().ZipArchiveWithEmptyFiles.Entries.Count, requiredFiles.Count);

        foreach (var requiredFile in requiredFiles)
        {
            var fileName = requiredFile.Key;
            var isRequired = requiredFile.Value;
            TestOutputHelper.WriteLine($"{fileName}: {isRequired}");

            var zipArchive = new DomainV2ZipArchiveBuilder()
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
