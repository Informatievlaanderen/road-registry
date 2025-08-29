namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using System.IO;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractsControllerTests
{
    [Fact(Skip = "//TODO-pr implement test")]
    public async Task When_uploading_an_extract_that_is_not_a_zip()
    {
        throw new NotImplementedException();
        // var formFile = EmbeddedResourceReader.ReadFormFile(new MemoryStream(), "name", "application/octet-stream");
        // var result = await Controller.UploadFeatureCompare(
        //     "not_a_guid_without_dashes",
        //     formFile,
        //     CancellationToken.None);
        //
        // Assert.IsType<UnsupportedMediaTypeResult>(result);
    }
}
