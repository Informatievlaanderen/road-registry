namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using Scenarios;
using Xunit.Abstractions;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public class ProjectionFilesTests : FeatureCompareTranslatorScenariosBase
{
    public ProjectionFilesTests(ITestOutputHelper testOutputHelper, ILogger<ZipArchiveFeatureCompareTranslator> logger)
        : base(testOutputHelper, logger)
    {
    }

    [Fact]
    public async Task EnsureProjectionFormatMustBeLambert08_TransactionZone()
    {
        var lambert72ProjectionFormatFileStream = new DomainV2ZipArchiveTestData().Fixture.CreateLambert72ProjectionFormatFileWithOneRecord();

        var zipArchive = new DomainV2ZipArchiveBuilder()
            .Build(transactionZoneProjectionFormatStream: lambert72ProjectionFormatFileStream);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().ContainSingle(x => x.Reason == nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08));
    }

    [Fact]
    public async Task EnsureProjectionFormatMustBeLambert08_RoadNode()
    {
        var lambert72ProjectionFormatFileStream = new DomainV2ZipArchiveTestData().Fixture.CreateLambert72ProjectionFormatFileWithOneRecord();

        var zipArchive = new DomainV2ZipArchiveBuilder()
            .Build(roadNodeProjectionFormatStream: lambert72ProjectionFormatFileStream);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().ContainSingle(x => x.File == "WEGKNOOP.PRJ" && x.Reason == nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08));
        ex.Problems.Should().ContainSingle(x => x.File == "EWEGKNOOP.PRJ" && x.Reason == nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08));
    }

    [Fact]
    public async Task EnsureProjectionFormatMustBeLambert08_RoadSegment()
    {
        var lambert72ProjectionFormatFileStream = new DomainV2ZipArchiveTestData().Fixture.CreateLambert72ProjectionFormatFileWithOneRecord();

        var zipArchive = new DomainV2ZipArchiveBuilder()
            .Build(roadSegmentProjectionFormatStream: lambert72ProjectionFormatFileStream);

        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() => TranslateReturnsExpectedResult(zipArchive, TranslatedChanges.Empty));
        ex.Problems.Should().ContainSingle(x => x.File == "WEGSEGMENT.PRJ" && x.Reason == nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08));
        ex.Problems.Should().ContainSingle(x => x.File == "EWEGSEGMENT.PRJ" && x.Reason == nameof(ProjectionFormatFileProblems.ProjectionFormatNotLambert08));
    }
}
