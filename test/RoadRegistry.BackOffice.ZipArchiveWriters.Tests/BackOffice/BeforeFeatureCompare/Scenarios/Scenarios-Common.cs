namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using FeatureCompare;
using Microsoft.Extensions.Logging;
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
        var sut = ZipArchiveFeatureCompareTranslatorFactory.Create();

        Assert.IsAssignableFrom<IZipArchiveFeatureCompareTranslator>(sut);
    }
    
    [Fact]
    public async Task TranslateArchiveCanNotBeNull()
    {
        var sut = ZipArchiveFeatureCompareTranslatorFactory.Create();

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.TranslateAsync(null, CancellationToken.None));
    }
}
