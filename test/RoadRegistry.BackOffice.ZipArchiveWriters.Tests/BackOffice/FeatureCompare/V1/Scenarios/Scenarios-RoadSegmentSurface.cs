namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.V1.Scenarios;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.FeatureCompare.V1;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V1;
using ValueObjects.ProblemCodes;
using Xunit.Abstractions;

public class RoadSegmentSurfaceScenarios : FeatureCompareTranslatorScenariosBase
{
    public RoadSegmentSurfaceScenarios(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task NotAdjacentFromToPositionShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var surfaceDbaseRecord1 = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord1.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord1.VANPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
                surfaceDbaseRecord1.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;
                var surfaceDbaseRecord2 = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord2.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord2.VANPOS.Value = 0;
                surfaceDbaseRecord2.TOTPOS.Value = 1;

                builder.DataSet.SurfaceDbaseRecords = new[] { surfaceDbaseRecord1, surfaceDbaseRecord2, builder.TestData.RoadSegment2SurfaceDbaseRecord }.ToList();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Surface.NotAdjacent, problem.Reason);
    }

    [Fact]
    public async Task NonZeroFromPositionShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value = 1;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Surface.FromPositionNotEqualToZero, problem.Reason);
    }

    [Fact]
    public async Task EqualFromToPositionShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var surfaceDbaseRecord1 = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord1.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord1.VANPOS.Value = 0;
                surfaceDbaseRecord1.TOTPOS.Value = 1;

                var surfaceDbaseRecord2 = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord2.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord2.VANPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
                surfaceDbaseRecord2.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;

                var surfaceDbaseRecord3 = builder.CreateRoadSegmentSurfaceDbaseRecord();
                surfaceDbaseRecord3.WS_OIDN.Value = builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value;
                surfaceDbaseRecord3.VANPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
                surfaceDbaseRecord3.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length;

                builder.DataSet.SurfaceDbaseRecords = new[] { surfaceDbaseRecord1, surfaceDbaseRecord2, surfaceDbaseRecord3, builder.TestData.RoadSegment2SurfaceDbaseRecord }.ToList();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        Assert.Contains(ex.Problems, problem => problem.Reason == ProblemCode.RoadSegment.Surface.HasLengthOfZero);
    }

    [Fact]
    public async Task ToPositionDifferentThanSegmentLengthShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = builder.TestData.RoadSegment1ShapeRecord.Geometry.Length - 1;
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Surface.ToPositionNotEqualToLength, problem.Reason);
    }

    [Fact]
    public async Task SingleRecordWithZeroToPositionShouldSucceed()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = 0;
            })
            .Build();

        await TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty);
    }

    [Fact]
    public async Task RoadSegmentsWithoutSurfaceAttributesShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                builder.DataSet.SurfaceDbaseRecords.Clear();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().ContainSingle(x => x.Reason == nameof(DbaseFileProblems.RoadSegmentsWithoutSurfaceAttributes));
    }

    [Fact]
    public async Task UsingUnknownRoadSegmentShouldGiveProblem()
    {
        var zipArchive = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, context) =>
            {
                var laneDbaseRecord3 = builder.CreateRoadSegmentSurfaceDbaseRecord();
                laneDbaseRecord3.WS_OIDN.Value = context.Fixture.CreateWhichIsDifferentThan(new RoadSegmentId(builder.TestData.RoadSegment1DbaseRecord.WS_OIDN.Value), new RoadSegmentId(builder.TestData.RoadSegment2DbaseRecord.WS_OIDN.Value));
                laneDbaseRecord3.VANPOS.Value = 0;
                laneDbaseRecord3.TOTPOS.Value = builder.TestData.RoadSegment2ShapeRecord.Geometry.Length;

                builder.DataSet.SurfaceDbaseRecords = new[] { builder.TestData.RoadSegment1SurfaceDbaseRecord, builder.TestData.RoadSegment2SurfaceDbaseRecord, laneDbaseRecord3 }.ToList();
            })
            .Build();

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        var problem = Assert.Single(ex.Problems);
        Assert.Equal(ProblemCode.RoadSegment.Missing, problem.Reason);
    }
}
