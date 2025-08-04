namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using System.IO;
using Microsoft.AspNetCore.Mvc;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task When_uploading_an_extract_that_is_not_a_zip()
    {
        //TODO-pr implement test
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
