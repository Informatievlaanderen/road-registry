namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using FeatureCompare;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureToggles;
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
        Assert.Throws<ArgumentNullException>(() => new ZipArchiveFeatureCompareTranslator(null, null, new UseValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunctionFeatureToggle(true)));
    }
    
    [Fact]
    public void IsZipArchiveFeatureCompareTranslator()
    {
        var sut = new ZipArchiveFeatureCompareTranslator(Encoding, Logger, new UseValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunctionFeatureToggle(true));

        Assert.IsAssignableFrom<IZipArchiveFeatureCompareTranslator>(sut);
    }
    
    [Fact]
    public async Task TranslateArchiveCanNotBeNull()
    {
        var sut = new ZipArchiveFeatureCompareTranslator(Encoding, Logger, new UseValidateRoadSegmentIntersectionsWithMissingGradeSeparatedJunctionFeatureToggle(true));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Translate(null, CancellationToken.None));
    }
}
