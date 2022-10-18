namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using Fixtures;

public class WhenIntegrationRoadNodesToZipArchiveWriter : IClassFixture<IntegrationRoadNodesToZipArchiveWriterFixture>
{
    public WhenIntegrationRoadNodesToZipArchiveWriter(IntegrationRoadNodesToZipArchiveWriterFixture fixture)
    {
        _fixture = fixture;
    }

    private readonly IntegrationRoadNodesToZipArchiveWriterFixture _fixture;

    [Fact(Skip = "For live debugging purposes")]
    public void ItShouldSucceeded()
    {
        Assert.NotNull(_fixture.Result);
    }
}
