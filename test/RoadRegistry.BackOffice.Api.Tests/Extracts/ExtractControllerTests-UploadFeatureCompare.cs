namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using FeatureToggles;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractControllerTests
{
    [Fact]
    public async Task When_uploading_an_extract_before_fc_that_is_not_a_zip()
    {
        var formFile = EmbeddedResourceReader.ReadFormFile(new MemoryStream(), "name", "application/octet-stream");
        var result = await Controller.UploadFeatureCompare(
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(false),
            "not_a_guid_without_dashes",
            formFile,
            CancellationToken.None);

        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }
}