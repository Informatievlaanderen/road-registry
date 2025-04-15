namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V2.Scenarios;

using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureCompare;
using RoadRegistry.BackOffice.FeatureCompare.V2;
using Xunit.Abstractions;

public class CommonScenarios: FeatureCompareTranslatorScenariosBase
{
    public CommonScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public void IsZipArchiveFeatureCompareTranslator()
    {
        var sut = ZipArchiveFeatureCompareTranslatorV2Builder.Create();

        Assert.IsAssignableFrom<IZipArchiveFeatureCompareTranslator>(sut);
    }

    [Fact]
    public async Task TranslateArchiveCanNotBeNull()
    {
        var sut = ZipArchiveFeatureCompareTranslatorV2Builder.Create();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TranslateAsync(null, CancellationToken.None));
    }
}
