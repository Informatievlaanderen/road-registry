namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V1.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare.V1;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using Xunit.Abstractions;

public class NumberedRoadScenarios : FeatureCompareTranslatorScenariosBase
{
    public NumberedRoadScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task UnknownRoadSegmentShouldGiveProblem()
    {
        var (zipArchive, expected) = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.WS_OIDN.Value = context.Fixture.CreateWhichIsDifferentThan(
                    builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value,
                    builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value);
            })
            .BuildWithResult(_ => TranslatedChanges.Empty);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, expected));
        Assert.Contains(ex.Problems, x => x.Reason == nameof(DbaseFileProblems.RoadSegmentIdOutOfRange));
    }

    [Fact]
    public async Task GivenDuplicateRecords_WhenSegmentIdentical_ThenOnlyOneIsDeleted()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value = builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.RICHTING.Value = builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.RICHTING.Value;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.VOLGNUMMER.Value = builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.VOLGNUMMER.Value;
            })
            .Build();

        var (translatedChanges, _) = await TranslateSucceeds(zipArchive);

        translatedChanges.Should().HaveCount(1);
        translatedChanges.Single().Should().BeOfType<RemoveRoadSegmentFromNumberedRoad>();
    }

    [Fact]
    public async Task GivenDuplicateRecords_WhenSegmentModified_ThenOnlyOneIsDeleted()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithExtract((builder, _) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatus.Unknown.Translation.Identifier;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.IDENT8.Value = builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.IDENT8.Value;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.RICHTING.Value = builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.RICHTING.Value;
                builder.TestData.RoadSegment1NumberedRoadDbaseRecord2.VOLGNUMMER.Value = builder.TestData.RoadSegment1NumberedRoadDbaseRecord1.VOLGNUMMER.Value;
            })
            .WithChange((builder, _) =>
            {
                builder.TestData.RoadSegment1DbaseRecord.STATUS.Value = RoadSegmentStatus.InUse.Translation.Identifier; // only to trigger a change
            })
            .Build();

        var (translatedChanges, _) = await TranslateSucceeds(zipArchive);

        translatedChanges.Should().HaveCount(2);
        translatedChanges.ToList()[0].Should().BeOfType<ModifyRoadSegment>();
        translatedChanges.ToList()[1].Should().BeOfType<RemoveRoadSegmentFromNumberedRoad>();
    }
}
