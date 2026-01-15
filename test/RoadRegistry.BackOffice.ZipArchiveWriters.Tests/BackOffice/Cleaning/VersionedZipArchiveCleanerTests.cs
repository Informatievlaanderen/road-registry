namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.Cleaning;

using System.IO.Compression;
using Moq;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V1;

public class VersionedZipArchiveCleanerTests
{
    [Theory]
    [InlineData(CleanResult.Changed, new []{ CleanResult.Changed})]
    [InlineData(CleanResult.NoChanges, new []{ CleanResult.NoChanges })]
    [InlineData(CleanResult.NoChanges, new []{ CleanResult.NotApplicable })]
    [InlineData(CleanResult.Changed, new []{ CleanResult.NotApplicable, CleanResult.Changed})]
    [InlineData(CleanResult.Changed, new []{ CleanResult.NotApplicable, CleanResult.Changed, CleanResult.NoChanges })]
    public async Task ReturnFirstNonNotApplicableResult(CleanResult expected, CleanResult[] results)
    {
        var archive = new ExtractV1ZipArchiveTestData().ZipArchiveWithEmptyFiles;

        var cleaner = new FakeVersionedZipArchiveCleaner(results);

        var cleanResult = await cleaner.CleanAsync(archive, CancellationToken.None);
        Assert.Equal(expected, cleanResult);
    }

    private sealed class FakeVersionedZipArchiveCleaner : VersionedZipArchiveCleaner
    {
        public FakeVersionedZipArchiveCleaner(params CleanResult[] results)
            : base(results.Select(BuildCleaner).ToArray())
        {
        }

        private static IZipArchiveCleaner BuildCleaner(CleanResult result)
        {
            var cleaner = new Mock<IZipArchiveCleaner>();
            cleaner
                .Setup(x => x.CleanAsync(It.IsAny<ZipArchive>(), It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(result));
            return cleaner.Object;
        }
    }
}
