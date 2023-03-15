namespace RoadRegistry.BackOffice.Api.Tests;

using Abstractions;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FeatureToggles;
using FluentValidation;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;
using Uploads;

public class UploadControllerTests : ControllerTests<UploadController>
{
    public UploadControllerTests(
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient,
        RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient, featureCompareBlobClient)
    {
    }

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

        var result = await Controller.PostUploadBeforeFeatureCompare(new UseFeatureCompareFeatureToggle(true), formFile, CancellationToken.None);
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

        var result = await Controller.PostUploadBeforeFeatureCompare(new UseFeatureCompareFeatureToggle(false), formFile, CancellationToken.None);
        Assert.IsType<NotFoundResult>(result);
    }

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

        var result = await Controller.PostUploadAfterFeatureCompare(formFile, CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Fact]
    public async Task When_uploading_an_externally_created_after_fc_file_that_is_a_valid_zip()
    {
        using (var sourceStream = new MemoryStream())
        {
            await using (var embeddedStream =
                         typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),
                             "valid-after.zip"))
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
            var result = await Controller.PostUploadAfterFeatureCompare(formFile, CancellationToken.None);

            Assert.IsType<OkResult>(result);

            var page = await StreamStore.ReadAllBackwards(Position.End, 1);
            var message = page.Messages.Single();
            Assert.Equal(nameof(RoadNetworkChangesArchiveAccepted), message.Type);

            var archiveAcceptedMessage = JsonConvert.DeserializeObject<RoadNetworkChangesArchiveAccepted>(await message.GetJsonData());

            Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(archiveAcceptedMessage!.ArchiveId)));
            var blob = await UploadBlobClient.GetBlobAsync(new BlobName(archiveAcceptedMessage.ArchiveId));
            await using (var openStream = await blob.OpenAsync())
            {
                var resultStream = new MemoryStream();
                await openStream.CopyToAsync(resultStream);
                resultStream.Position = 0;
                sourceStream.Position = 0;

                Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
            }
        }
    }

    [Fact]
    public async Task When_uploading_an_externally_created_after_fc_file_that_is_an_empty_zip()
    {
        using (var sourceStream = new MemoryStream())
        {
            await using (var embeddedStream =
                         typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),
                             "empty.zip"))
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
                var result = await Controller.PostUploadAfterFeatureCompare(formFile, CancellationToken.None);
                Assert.IsType<OkResult>(result);
            }
            catch (ValidationException ex)
            {
                var validationFileProblems = ex.Errors.Select(ex => ex.PropertyName);

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

    [Fact]
    public async Task When_uploading_an_externally_created_before_fc_file_that_is_a_valid_zip()
    {
        using (var sourceStream = new MemoryStream())
        {
            await using (var embeddedStream =
                         typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),
                             "valid-before.zip"))
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
            var result = await Controller.PostUploadBeforeFeatureCompare(new UseFeatureCompareFeatureToggle(true), formFile, CancellationToken.None);

            var typedResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UploadExtractFeatureCompareResponseBody>(typedResult.Value);

            Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(response.ArchiveId)));
            var blob = await UploadBlobClient.GetBlobAsync(new BlobName(response.ArchiveId));
            await using (var openStream = await blob.OpenAsync())
            {
                var resultStream = new MemoryStream();
                openStream.CopyTo(resultStream);
                resultStream.Position = 0;
                sourceStream.Position = 0;

                Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
            }
        }
    }

    [Fact]
    public async Task When_uploading_an_externally_created_before_fc_file_that_is_an_empty_zip()
    {
        using (var sourceStream = new MemoryStream())
        {
            await using (var embeddedStream =
                         typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),
                             "empty.zip"))
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
                await Controller.PostUploadBeforeFeatureCompare(new UseFeatureCompareFeatureToggle(true), formFile, CancellationToken.None);
                throw new ValidationException("This should not be reachable");
            }
            catch (ValidationException ex)
            {
                var validationFileProblems = ex.Errors.Select(ex => ex.PropertyName);

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
}
