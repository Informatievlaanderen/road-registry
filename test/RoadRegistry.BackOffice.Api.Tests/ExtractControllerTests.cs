namespace RoadRegistry.BackOffice.Api.Tests;

using Api.Extracts;
using AutoFixture;
using BackOffice.Abstractions;
using BackOffice.Abstractions.Exceptions;
using BackOffice.Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Editor.Schema;
using FluentValidation;
using Framework.Containers;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Tests.Abstractions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Tests.BackOffice;
using SqlStreamStore;
using Xunit.Sdk;
using GeometryTranslator = BackOffice.GeometryTranslator;
using Point = NetTopologySuite.Geometries.Point;
using Position = SqlStreamStore.Streams.Position;

[Collection(nameof(SqlServerCollection))]
public class ExtractControllerTests : ControllerTests<ExtractsController>, IAsyncLifetime
{
    private readonly Fixture _fixture;
    private readonly SqlServer _sqlServerFixture;
    private EditorContext _editorContext;

    public ExtractControllerTests(
        SqlServer sqlServerFixture,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient,
        RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient, featureCompareBlobClient)
    {
        _sqlServerFixture = sqlServerFixture;
        _fixture = new Fixture();
        _fixture.CustomizeExternalExtractRequestId();
        _fixture.CustomizeRoadNetworkExtractGeometry();
    }

    // TODO: Figure out how to use Geometry with InMemoryDatabase (or switch to integration testing)
    // [Fact]
    // public async Task When_downloading_an_extract_using_an_unknown_download_id()
    // {
    //     var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
    //     var validator =
    //         new DownloadExtractRequestBodyValidator(wktReader,
    //             new NullLogger<DownloadExtractRequestBodyValidator>());
    //     var controller = new ExtractsController(SystemClock.Instance,Dispatch.Using(_resolver), _downloadClient, _uploadClient, wktReader, validator)
    //     {
    //         ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
    //     };
    //     var context = new EditorContext();
    //     var response = await controller.GetDownload(
    //         context,
    //         new ExtractDownloadsOptions(),
    //         "8393620921e14ad49813dacb59ba850d");
    //     Assert.IsType<NotFoundResult>(response);
    // }
    //
    // [Fact]
    // public async Task When_downloading_an_extract_that_is_not_yet_available(){}
    //
    // [Fact]
    // public async Task When_downloading_an_extract_that_is_available(){}

    public async Task InitializeAsync()
    {
        _editorContext = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync());
    }

    public async Task DisposeAsync()
    {
        await _editorContext.DisposeAsync();
    }

    [Fact]
    public async Task When_requesting_an_extract_for_the_first_time()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        var contour = _fixture.Create<RoadNetworkExtractGeometry>();
        var response = await Controller.PostDownloadRequest(new DownloadExtractRequestBody
        {
            RequestId = externalExtractRequestId,
            Contour = writer.Write((Geometry)GeometryTranslator.Translate(contour))
        }, CancellationToken.None);
        var result = Assert.IsType<AcceptedResult>(response);
        Assert.IsType<DownloadExtractResponse>(result.Value);
    }

    [Fact]
    public async Task When_requesting_an_extract_without_request_id()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };
        //var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
        //var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
        //var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        //var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

        var contour = _fixture.Create<RoadNetworkExtractGeometry>();
        try
        {
            await Controller.PostDownloadRequest(new DownloadExtractRequestBody
            {
                RequestId = null,
                Contour = writer.Write((Geometry)GeometryTranslator.Translate(contour))
            }, CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_requesting_an_extract_without_contour()
    {
        //var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
        //var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
        //var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        //var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        try
        {
            await Controller.PostDownloadRequest(new DownloadExtractRequestBody
            {
                RequestId = externalExtractRequestId,
                Contour = null
            },
                CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_requesting_an_extract_with_a_contour_that_is_not_a_multipolygon()
    {
        var writer = new WKTWriter
        {
            PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
        };
        //var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
        //var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
        //var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        //var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

        var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
        try
        {
            await Controller.PostDownloadRequest(new DownloadExtractRequestBody
            {
                RequestId = externalExtractRequestId,
                Contour = writer.Write(new Point(1.0, 2.0))
            }, CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_downloading_an_extract_using_an_malformed_download_id()
    {
        try
        {
            await Controller.GetDownload(
                "not_a_guid_without_dashes",
                new ExtractDownloadsOptions(),
                CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_uploading_an_extract_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        try
        {
            //await Controller.PostUpload(
            //    "not_a_guid_without_dashes",
            //    formFile,
            //    CancellationToken.None);
            await Controller.PostFeatureCompareUpload(
                "not_a_guid_without_dashes",
                formFile,
                CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (UploadExtractNotFoundException)
        {
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_uploading_an_extract_that_is_an_empty_zip()
    {
        try
        {
            using (var sourceStream = new MemoryStream())
            {
                await using (var embeddedStream =
                             typeof(ExtractControllerTests).Assembly.GetManifestResourceStream(
                                 typeof(ExtractControllerTests),
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

                var result = await Controller.PostFeatureCompareUpload(
                        "not_a_guid_without_dashes",
                        formFile,
                        CancellationToken.None);

                Assert.IsType<OkResult>(result);

                var page = await StreamStore.ReadAllForwards(Position.Start, 1);
                var message = Assert.Single(page.Messages);
                Assert.Equal(nameof(RoadNetworkExtractChangesArchiveUploaded), message.Type);
                var uploaded =
                    JsonConvert.DeserializeObject<RoadNetworkExtractChangesArchiveUploaded>(
                        await message.GetJsonData());

                Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(uploaded.ArchiveId)));
                var blob = await UploadBlobClient.GetBlobAsync(new BlobName(uploaded.ArchiveId));
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
        catch (UploadExtractNotFoundException) { }
        catch (ValidationException)
        {
        }
    }
}
