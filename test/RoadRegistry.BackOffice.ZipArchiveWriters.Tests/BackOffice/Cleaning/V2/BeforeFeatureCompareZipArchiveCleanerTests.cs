namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.Cleaning.V2;

using Extracts;
using FluentAssertions;
using RoadRegistry.BackOffice.FeatureCompare.V2;
using RoadRegistry.BackOffice.FeatureCompare.V2.Readers;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using Uploads;
using ZipArchiveWriters.Cleaning.V2;

public class BeforeFeatureCompareZipArchiveCleanerTests
{
    [Fact]
    public async Task WhenChanged_ThenReadShouldContainChanges()
    {
        // Arrange
        var cleaner = new BeforeFeatureCompareZipArchiveCleaner(FileEncoding.UTF8);

        var archive = new ExtractsZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value = null;
                builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = null;
            })
            .Build();

        // Act
        var cleanResult = await cleaner.CleanAsync(archive, CancellationToken.None);

        // Assert
        cleanResult.Should().Be(CleanResult.Changed);

        var reader = new RoadSegmentLaneFeatureCompareFeatureReader(FileEncoding.UTF8);
        var (features, problems) = reader.Read(archive, FeatureType.Change, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

        var relevantProblems = problems.Where(x => x.Reason != nameof(DbaseFileProblems.RoadSegmentMissing)).ToList();

        relevantProblems.Should().BeEmpty();
        features.TrueForAll(x => x.Attributes.ToPosition > RoadSegmentPosition.Zero).Should().BeTrue();
    }
}
