namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using Fixtures;

public class WhenRoadNetworkExtractToZipArchiveWriter : IClassFixture<RoadNetworkExtractToZipArchiveWriterFixture>
{
    private readonly RoadNetworkExtractToZipArchiveWriterFixture _fixture;

    public WhenRoadNetworkExtractToZipArchiveWriter(RoadNetworkExtractToZipArchiveWriterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "For live debugging purposes")]
    public void ItShouldSucceeded()
    {
        Assert.NotNull(_fixture.Result);
    }
}