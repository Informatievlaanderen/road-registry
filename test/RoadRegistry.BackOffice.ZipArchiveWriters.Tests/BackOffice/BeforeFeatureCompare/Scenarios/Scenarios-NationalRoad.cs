namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;

using DutchTranslations;
using Exceptions;
using FeatureCompare;
using Microsoft.Extensions.Logging;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Xunit.Abstractions;

public class NationalRoadScenarios : FeatureCompareTranslatorScenariosBase
{
    public NationalRoadScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
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
                builder.TestData.RoadSegment1NationalRoadDbaseRecord2.IDENT2.Value = builder.TestData.RoadSegment1NationalRoadDbaseRecord1.IDENT2.Value;

                expectedTranslatedProblemMessage = $"De dbase record 1 met NW_OIDN {builder.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value} heeft hetzelfde WS_OIDN en IDENT2 als de dbase record 2 met NW_OIDN {builder.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value}.";
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(nameof(DbaseFileProblems.NationalRoadNotUnique), problem.Reason);

        Assert.Equal(expectedTranslatedProblemMessage, FileProblemTranslator.Dutch(problem.Translate()).Message);
    }

    [Fact]
    public async Task UnknownRoadSegmentShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1NationalRoadDbaseRecord1.WS_OIDN.Value = context.Fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.RoadSegmentIdOutOfRange));
    }
}
