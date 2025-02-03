namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.BeforeFeatureCompare.Scenarios;
using Core.ProblemCodes;
using Exceptions;
using FeatureCompare;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using Xunit.Abstractions;

public class RoadSegmentLaneScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadSegmentLaneScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task NotAdjacentFromToPositionShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var laneDbaseRecord1 = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord1.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                laneDbaseRecord1.VANPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
                laneDbaseRecord1.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
                var laneDbaseRecord2 = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord2.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                laneDbaseRecord2.VANPOS.Value = 0;
                laneDbaseRecord2.TOTPOS.Value = 1;

                builder.DataSet.LaneDbaseRecords = new[] { laneDbaseRecord1, laneDbaseRecord2, builder.TestData.RoadSegment2LaneDbaseRecord }.ToList();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Lane.NotAdjacent, problem.Reason);
    }

    [Fact]
    public async Task NonZeroFromPositionShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value = 1;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Lane.FromPositionNotEqualToZero, problem.Reason);
    }

    [Fact]
    public async Task EqualFromToPositionShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var laneDbaseRecord1 = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord1.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                laneDbaseRecord1.VANPOS.Value = 0;
                laneDbaseRecord1.TOTPOS.Value = 1;

                var laneDbaseRecord2 = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord2.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                laneDbaseRecord2.VANPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
                laneDbaseRecord2.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;

                var laneDbaseRecord3 = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord3.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                laneDbaseRecord3.VANPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
                laneDbaseRecord3.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;

                builder.DataSet.LaneDbaseRecords = new[] { laneDbaseRecord1, laneDbaseRecord2, laneDbaseRecord3, builder.TestData.RoadSegment2LaneDbaseRecord }.ToList();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));

        Assert.Contains(ex.Problems, problem => problem.Reason == ProblemCode.RoadSegment.Lane.HasLengthOfZero);
    }

    [Fact]
    public async Task ToPositionDifferentThanSegmentLengthShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Lane.ToPositionNotEqualToLength, problem.Reason);
    }

    [Fact]
    public async Task SingleRecordWithZeroToPositionShouldSucceed()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = 0;
            })
            .Build();

        await TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty);
    }

    [Fact]
    public async Task RoadSegmentsWithoutLaneAttributesShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.DataSet.LaneDbaseRecords.Clear();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().ContainSingle(x => x.Reason == nameof(DbaseFileProblems.RoadSegmentsWithoutLaneAttributes));
    }

    [Fact]
    public async Task UsingUnknownRoadSegmentShouldGiveProblem()
    {
        var zipArchive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var laneDbaseRecord3 = builder.CreateRoadSegmentLaneDbaseRecord();
                laneDbaseRecord3.WS_OIDN.Value = context.Fixture.CreateWhichIsDifferentThan(
                    new RoadSegmentId(builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value),
                    new RoadSegmentId(builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value));
                laneDbaseRecord3.VANPOS.Value = 0;
                laneDbaseRecord3.TOTPOS.Value = builder.TestData.RoadSegment2ShapeRecord.Geometry.Length;

                builder.DataSet.LaneDbaseRecords = new[] { builder.TestData.RoadSegment1LaneDbaseRecord, builder.TestData.RoadSegment2LaneDbaseRecord, laneDbaseRecord3 }.ToList();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Missing, problem.Reason);
    }
}
