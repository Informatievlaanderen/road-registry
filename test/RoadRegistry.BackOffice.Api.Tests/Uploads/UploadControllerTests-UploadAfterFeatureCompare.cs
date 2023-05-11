namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

public partial class UploadControllerTests
{
    [Fact]
    public async Task When_uploading_an_after_fc_file_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.UploadAfterFeatureCompare(formFile, CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Fact]
    public async Task When_uploading_an_externally_created_after_fc_file_that_is_a_valid_zip()
    {
        using var sourceStream = new MemoryStream();
        await using (var embeddedStream = typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests), "valid-after.zip"))
        {
            await embeddedStream!.CopyToAsync(sourceStream);
        }
        sourceStream.Position = 0;

        var formFile = new FormFile(sourceStream, 0L, sourceStream.Length, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/zip") }
            })
        };
        var result = await Controller.UploadAfterFeatureCompare(formFile, CancellationToken.None);

        Assert.IsType<OkResult>(result);

        var page = await StreamStore.ReadAllBackwards(Position.End, 1);
        var message = page.Messages.Single();
        Assert.Equal(nameof(UploadRoadNetworkChangesArchive), message.Type);

        var changesArchiveMessage = JsonConvert.DeserializeObject<UploadRoadNetworkChangesArchive>(await message.GetJsonData());

        Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(changesArchiveMessage!.ArchiveId)));
        var blob = await UploadBlobClient.GetBlobAsync(new BlobName(changesArchiveMessage.ArchiveId));
        await using (var openStream = await blob.OpenAsync())
        {
            var resultStream = new MemoryStream();
            await openStream.CopyToAsync(resultStream);
            resultStream.Position = 0;
            sourceStream.Position = 0;

            Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
        }
    }

    [Fact]
    public async Task When_uploading_an_externally_created_after_fc_file_that_is_an_empty_zip()
    {
        using var sourceStream = new MemoryStream();
        await using (var embeddedStream = typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),  "empty.zip")) {
            embeddedStream.CopyTo(sourceStream);
        }
        sourceStream.Position = 0;

        var formFile = new FormFile(sourceStream, 0L, sourceStream.Length, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/zip") }
            })
        };

        try
        {
            var result = await Controller.UploadAfterFeatureCompare(formFile, CancellationToken.None);
            Assert.IsType<OkResult>(result);
        }
        catch (ZipArchiveValidationException ex)
        {
            var validationFileProblems = ex.Problems.Select(fileProblem => fileProblem.File).ToArray();

            Assert.Contains("TRANSACTIEZONES.DBF", validationFileProblems);
            Assert.Contains("WEGKNOOP_ALL.DBF", validationFileProblems);
            Assert.Contains("WEGKNOOP_ALL.SHP", validationFileProblems);
            Assert.Contains("WEGKNOOP_ALL.PRJ", validationFileProblems);
            Assert.Contains("WEGSEGMENT_ALL.DBF", validationFileProblems);
            Assert.Contains("ATTRIJSTROKEN_ALL.DBF", validationFileProblems);
            Assert.Contains("ATTWEGBREEDTE_ALL.DBF", validationFileProblems);
            Assert.Contains("ATTWEGVERHARDING_ALL.DBF", validationFileProblems);
            Assert.Contains("WEGSEGMENT_ALL.SHP", validationFileProblems);
            Assert.Contains("WEGSEGMENT_ALL.PRJ", validationFileProblems);
            Assert.Contains("ATTEUROPWEG_ALL.DBF", validationFileProblems);
            Assert.Contains("ATTNATIONWEG_ALL.DBF", validationFileProblems);
            Assert.Contains("ATTGENUMWEG_ALL.DBF", validationFileProblems);
            Assert.Contains("RLTOGKRUISING_ALL.DBF", validationFileProblems);
        }
    }
}
