namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V2.Scenarios;

using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.DutchTranslations;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare.V2;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Tests.BackOffice;
using Xunit.Abstractions;

public class EuropeanRoadScenarios : FeatureCompareTranslatorScenariosBase
{
    public EuropeanRoadScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task RecordsShouldBeUnique()
    {
        string expectedTranslatedProblemMessage = null;

        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EUNUMMER.Value = builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EUNUMMER.Value;

                expectedTranslatedProblemMessage = $"De dbase record 1 met EU_OIDN {builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.EU_OIDN.Value} heeft hetzelfde WS_OIDN en EUNUMMER als de dbase record 2 met EU_OIDN {builder.TestData.RoadSegment1EuropeanRoadDbaseRecord2.EU_OIDN.Value}.";
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.EuropeanRoadNotUnique), problem.Reason);

        Assert.Equal(expectedTranslatedProblemMessage, FileProblemTranslator.Dutch(problem.Translate()).Message);
    }

    [Fact]
    public async Task UnknownRoadSegmentShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1EuropeanRoadDbaseRecord1.WS_OIDN.Value = context.Fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.RoadSegmentIdOutOfRange));
    }
}
