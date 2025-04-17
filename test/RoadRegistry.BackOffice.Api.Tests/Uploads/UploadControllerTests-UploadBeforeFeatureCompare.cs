namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using Microsoft.AspNetCore.Mvc;

public partial class UploadControllerTests
{
    [Fact]
    public async Task WhenNotZipFile_ThenUnsupportedMediaTypeResult()
    {
        var formFile = EmbeddedResourceReader.ReadFormFile(new MemoryStream(), "name", "application/octet-stream");
        var result = await Controller.UploadBeforeFeatureCompare(
            formFile,
            CancellationToken.None);

        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }
}
