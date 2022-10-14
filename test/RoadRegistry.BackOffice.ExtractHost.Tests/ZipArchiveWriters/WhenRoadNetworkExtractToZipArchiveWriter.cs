namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using Fixtures;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

public class WhenRoadNetworkExtractToZipArchiveWriter : IClassFixture<RoadNetworkExtractToZipArchiveWriterFixture>
{
    private readonly RoadNetworkExtractToZipArchiveWriterFixture _fixture;

    public WhenRoadNetworkExtractToZipArchiveWriter(RoadNetworkExtractToZipArchiveWriterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ItShouldSucceeded()
    {
        Assert.NotNull(_fixture.Result);
    }
}
