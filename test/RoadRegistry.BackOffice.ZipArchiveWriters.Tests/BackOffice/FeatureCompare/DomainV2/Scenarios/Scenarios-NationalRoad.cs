namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2.Scenarios;

using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.DutchTranslations;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using RoadSegment.Changes;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

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

        var zipArchive = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1NationalRoadDbaseRecord2.NWNUMMER.Value = builder.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value;

                expectedTranslatedProblemMessage = $"De dbase record 1 met NW_OIDN {builder.TestData.RoadSegment1NationalRoadDbaseRecord1.NW_OIDN.Value} heeft hetzelfde WS_TEMPID en NWNUMMER als de dbase record 2 met NW_OIDN {builder.TestData.RoadSegment1NationalRoadDbaseRecord2.NW_OIDN.Value}.";
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
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1NationalRoadDbaseRecord1.WS_TEMPID.Value = context.Fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_TEMPID.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_TEMPID.Value);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.RoadSegmentIdOutOfRange));
    }

    [Fact]
    public async Task WhenSegmentIsKeptAndOnlyNationalNumberRemoved_ThenOnlyNationalNumberRemoved()
    {
        var (zipArchive, expected) = new DomainV2ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.DataSet.NationalRoadDbaseRecords.RemoveAt(0);
            })
            .BuildWithResult(context => TranslatedChanges.Empty
                .AppendChange(
                    new RemoveRoadSegmentFromNationalRoadChange
                    {
                        RoadSegmentId = new RoadSegmentId(context.Extract.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                        Number = NationalRoadNumber.Parse(context.Extract.TestData.RoadSegment1NationalRoadDbaseRecord1.NWNUMMER.Value!)
                    }
                ));

        await TranslateReturnsExpectedResult(zipArchive, expected);
    }
}
