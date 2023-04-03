namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using Fixtures;

public class WhenIntegrationToZipArchiveWriter : IClassFixture<
    IntegrationToZipArchiveWriterFixture>
{
    private readonly IntegrationToZipArchiveWriterFixture _fixture;

    public WhenIntegrationToZipArchiveWriter(IntegrationToZipArchiveWriterFixture fixture)
    {
        _fixture = fixture;
    }

    //[Fact(Skip = "For live debugging purposes")]
    public void ItShouldSucceeded()
    {
        Assert.NotNull(_fixture.Result);
    }
}
