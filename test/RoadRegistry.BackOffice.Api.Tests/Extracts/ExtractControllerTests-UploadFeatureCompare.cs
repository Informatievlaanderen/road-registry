namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using FeatureToggles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

public partial class ExtractControllerTests
{
    [Fact]
    public async Task When_uploading_an_extract_before_fc_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.UploadFeatureCompare(
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(false),
            "not_a_guid_without_dashes",
            formFile,
            CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }
}