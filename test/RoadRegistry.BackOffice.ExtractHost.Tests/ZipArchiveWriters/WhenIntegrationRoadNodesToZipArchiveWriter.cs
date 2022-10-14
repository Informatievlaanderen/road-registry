namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using Fixtures;

public class WhenIntegrationRoadNodesToZipArchiveWriter : IClassFixture<IntegrationRoadNodesToZipArchiveWriterFixture>
{
    private readonly IntegrationRoadNodesToZipArchiveWriterFixture _fixture;

    public WhenIntegrationRoadNodesToZipArchiveWriter(IntegrationRoadNodesToZipArchiveWriterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void ItShouldSucceeded()
    {
        Assert.NotNull(_fixture.Result);
    }
}
