namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V3.Scenarios;

using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureCompare.V3;
using Uploads;
using Xunit.Abstractions;
using IZipArchiveFeatureCompareTranslator = RoadRegistry.BackOffice.FeatureCompare.V3.IZipArchiveFeatureCompareTranslator;

public class CommonScenarios: FeatureCompareTranslatorScenariosBase
{
    public CommonScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public void IsZipArchiveFeatureCompareTranslator()
    {
        var sut = ZipArchiveFeatureCompareTranslatorV3Builder.Create();

        Assert.IsAssignableFrom<IZipArchiveFeatureCompareTranslator>(sut);
    }

    [Fact]
    public async Task TranslateArchiveCanNotBeNull()
    {
        var sut = ZipArchiveFeatureCompareTranslatorV3Builder.Create();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TranslateAsync(null, ZipArchiveMetadata.Empty, CancellationToken.None));
    }
}
