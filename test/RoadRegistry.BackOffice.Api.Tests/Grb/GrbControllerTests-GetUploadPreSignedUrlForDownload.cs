namespace RoadRegistry.BackOffice.Api.Tests.Grb;

using Abstractions.Exceptions;
using Editor.Schema;
using Exceptions;

public partial class GrbControllerTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid_guid")]
    [InlineData("e33d6fa8-f89a-4342-8353-6373756c4463")]
    public async Task When_requesting_presigned_upload_url_with_invalid_downloadid(string downloadId)
    {
        await Assert.ThrowsAsync<InvalidGuidValidationException>(() => Controller.UploadForDownload(
            downloadId,
            new FakeEditorContextFactory().CreateDbContext(),
            CancellationToken.None
        ));
    }

    [Fact]
    public async Task When_requesting_presigned_upload_url_with_unknown_extract()
    {
        var downloadId = Guid.NewGuid();

        await Assert.ThrowsAsync<ExtractRequestNotFoundException>(() => Controller.UploadForDownload(
            new DownloadId(downloadId),
            new FakeEditorContextFactory().CreateDbContext(),
            CancellationToken.None
        ));
    }
}
