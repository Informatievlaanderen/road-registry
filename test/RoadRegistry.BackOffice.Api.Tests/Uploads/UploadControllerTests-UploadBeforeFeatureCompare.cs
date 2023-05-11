namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using BackOffice.Abstractions.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using FeatureToggles;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

public partial class UploadControllerTests
{
    [Fact]
    public async Task When_uploading_a_before_fc_file_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.UploadBeforeFeatureCompare(
            new UseFeatureCompareFeatureToggle(true),
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
            formFile,
            CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Fact]
    public async Task When_uploading_a_before_fc_file_with_featuretoggle_disabled()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.UploadBeforeFeatureCompare(
            new UseFeatureCompareFeatureToggle(false),
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(false),
            formFile, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task When_uploading_an_externally_created_before_fc_file_that_is_a_valid_zip()
    {
        using var sourceStream = new MemoryStream();
        await using (var embeddedStream = typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests), "valid-before.zip"))
        {
            await embeddedStream.CopyToAsync(sourceStream);
        }
        sourceStream.Position = 0;

        var formFile = new FormFile(sourceStream, 0L, sourceStream.Length, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/zip") }
            })
        };
        var result = await Controller.UploadBeforeFeatureCompare(
            new UseFeatureCompareFeatureToggle(true),
            new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
            formFile, CancellationToken.None);

        var typedResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UploadExtractFeatureCompareResponse>(typedResult.Value);

        Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(response.ArchiveId)));
        var blob = await UploadBlobClient.GetBlobAsync(new BlobName(response.ArchiveId));
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
    public async Task When_uploading_an_externally_created_before_fc_file_that_is_an_empty_zip()
    {
        using var sourceStream = new MemoryStream();
        await using (var embeddedStream = typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests), "empty.zip"))
        {
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
            await Controller.UploadBeforeFeatureCompare(
                new UseFeatureCompareFeatureToggle(true),
                new UseZipArchiveFeatureCompareTranslatorFeatureToggle(true),
                formFile,
                CancellationToken.None);
            throw new ValidationException("This should not be reachable");
        }
        catch (ZipArchiveValidationException ex)
        {
            var validationFileProblems = ex.Problems.Select(fileProblem => fileProblem.File).ToArray();

            Assert.Contains("WEGKNOOP.SHP", validationFileProblems);
            Assert.Contains("WEGKNOOP.DBF", validationFileProblems);
            Assert.Contains("WEGSEGMENT.SHP", validationFileProblems);
            Assert.Contains("WEGSEGMENT.DBF", validationFileProblems);
            Assert.Contains("ATTEUROPWEG.DBF", validationFileProblems);
            Assert.Contains("ATTNATIONWEG.DBF", validationFileProblems);
            Assert.Contains("ATTGENUMWEG.DBF", validationFileProblems);
            Assert.Contains("ATTRIJSTROKEN.DBF", validationFileProblems);
            Assert.Contains("ATTWEGBREEDTE.DBF", validationFileProblems);
            Assert.Contains("ATTWEGVERHARDING.DBF", validationFileProblems);
            Assert.Contains("RLTOGKRUISING.DBF", validationFileProblems);
            Assert.Contains("TRANSACTIEZONES.DBF", validationFileProblems);
        }
    }
}
