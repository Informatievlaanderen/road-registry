namespace RoadRegistry.BackOffice.Api.Tests;

using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NodaTime;
using RoadRegistry.BackOffice.Api.Tests.Abstractions;
using RoadRegistry.BackOffice.Extracts;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System.IO.Compression;
using System.Text;
using BackOffice.Abstractions.Uploads;
using FluentValidation;
using Uploads;

public class UploadControllerTests : ControllerTests<UploadController>
{
    private readonly ISqsQueuePublisher _sqsQueuePublisher;

    public UploadControllerTests(IMediator mediator, IStreamStore streamStore, ISqsQueuePublisher sqsQueuePublisher, RoadNetworkUploadsBlobClient uploadClient, RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient)
    {
        _sqsQueuePublisher = sqsQueuePublisher;
    }

    [Fact]
    public async Task When_uploading_a_file_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.PostUpload(formFile, CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Fact]
    public async Task When_uploading_an_externally_created_file_that_is_an_empty_zip()
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
            var result = await Controller.PostUpload(formFile, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);

            var typedResult = (BadRequestObjectResult)result;
            var validationException = typedResult.Value as ValidationException;

            var validationFileProblems = validationException.Errors.Select(ex => ex.PropertyName);

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

    [Fact]
    public async Task When_uploading_an_externally_created_file_that_is_a_valid_zip()
    {
        using (var sourceStream = new MemoryStream())
        {
            await using (var embeddedStream =
                         typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),
                             "valid.zip"))
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
            var result = await Controller.PostUpload(formFile, CancellationToken.None);

            var typedResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<UploadExtractFeatureCompareResponse>(typedResult.Value);

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
}
