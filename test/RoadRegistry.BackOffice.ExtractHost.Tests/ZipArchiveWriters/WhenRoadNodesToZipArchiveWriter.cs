namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using Fixtures;

public class WhenRoadNodesToZipArchiveWriter : IClassFixture<RoadNodesToZipArchiveWriterFixture>
{
    private readonly RoadNodesToZipArchiveWriterFixture _fixture;

    public WhenRoadNodesToZipArchiveWriter(RoadNodesToZipArchiveWriterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "For live debugging purposes")]
    public void ItShouldSucceeded()
    {
        Assert.NotNull(_fixture.Result);
    }
}
