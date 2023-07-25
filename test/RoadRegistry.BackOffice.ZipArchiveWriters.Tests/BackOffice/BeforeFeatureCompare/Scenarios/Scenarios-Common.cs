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
    public void EncodingCanNotBeNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveFeatureCompareTranslator(null, null));
    }
    
    [Fact]
    public void IsZipArchiveFeatureCompareTranslator()
    {
        var sut = new ZipArchiveFeatureCompareTranslator(Encoding, Logger);

        Assert.IsAssignableFrom<IZipArchiveFeatureCompareTranslator>(sut);
    }
    
    [Fact]
    public async Task TranslateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveFeatureCompareTranslator(Encoding, Logger);

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Translate(null, CancellationToken.None));
    }
}
