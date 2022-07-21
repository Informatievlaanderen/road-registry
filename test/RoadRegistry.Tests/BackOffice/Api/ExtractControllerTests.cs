namespace RoadRegistry.BackOffice.Api
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice.Extracts;
    using BackOffice.Framework;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Configuration;
    using Editor.Schema;
    using Extracts;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Primitives;
    using NetTopologySuite;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;
    using NodaTime;
    using RoadRegistry.Framework.Containers;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Sdk;

    [Collection(nameof(SqlServerCollection))]
    public class ExtractControllerTests : IAsyncLifetime
    {
        private readonly SqlServer _sqlServerFixture;
        private readonly Fixture _fixture;
        private readonly CommandHandlerResolver _resolver;
        private readonly RoadNetworkExtractDownloadsBlobClient _downloadClient;
        private readonly RoadNetworkExtractUploadsBlobClient _uploadClient;
        private EditorContext _editorContext;

        public ExtractControllerTests(SqlServer sqlServerFixture)
        {
            _sqlServerFixture = sqlServerFixture;
            _fixture = new Fixture();
            _fixture.CustomizeExternalExtractRequestId();
            _fixture.CustomizeRoadNetworkExtractGeometry();
            var client = new MemoryBlobClient();
            _downloadClient = new RoadNetworkExtractDownloadsBlobClient(client);
            _uploadClient = new RoadNetworkExtractUploadsBlobClient(client);
            _resolver = Resolve.WhenEqualToMessage(
                new RoadNetworkExtractCommandModule(
                    _uploadClient,
                    new InMemoryStreamStore(),
                    new FakeRoadNetworkSnapshotReader(),
                    new ZipArchiveValidator(Encoding.UTF8),
                    SystemClock.Instance
                )
            );
        }

        [Fact]
        public async Task When_requesting_an_extract_for_the_first_time()
        {
            var writer = new WKTWriter
            {
                PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
            };

            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
            var contour = _fixture.Create<Messages.RoadNetworkExtractGeometry>();
            var response = await controller.PostDownloadRequest(new DownloadExtractRequestBody
            {
                RequestId = externalExtractRequestId,
                Contour = writer.Write((NetTopologySuite.Geometries.Geometry)BackOffice.GeometryTranslator.Translate(contour))
            });
            var result = Assert.IsType<AcceptedResult>(response);
            Assert.IsType<DownloadExtractResponseBody>(result.Value);
        }

        [Fact]
        public async Task When_requesting_an_extract_without_request_id()
        {
            var writer = new WKTWriter
            {
                PrecisionModel = GeometryConfiguration.GeometryFactory.PrecisionModel
            };
            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var contour = _fixture.Create<Messages.RoadNetworkExtractGeometry>();
            try
            {
                await controller.PostDownloadRequest(new DownloadExtractRequestBody
                {
                    RequestId = null,
                    Contour = writer.Write((NetTopologySuite.Geometries.Geometry)BackOffice.GeometryTranslator.Translate(contour))
                });
                throw new XunitException("Expected a validation exception but did not receive any");
            }
            catch (ValidationException)
            {
            }
        }

        [Fact]
        public async Task When_requesting_an_extract_without_contour()
        {
            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
            try
            {
                await controller.PostDownloadRequest(new DownloadExtractRequestBody
                {
                    RequestId = externalExtractRequestId,
                    Contour = null
                });
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
            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var externalExtractRequestId = _fixture.Create<ExternalExtractRequestId>();
            try
            {
                await controller.PostDownloadRequest(new DownloadExtractRequestBody
                {
                    RequestId = externalExtractRequestId,
                    Contour = writer.Write(new NetTopologySuite.Geometries.Point(1.0, 2.0))
                });
                throw new XunitException("Expected a validation exception but did not receive any");
            }
            catch (ValidationException)
            {
            }
        }

        [Fact]
        public async Task When_downloading_an_extract_using_an_malformed_download_id()
        {
            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var context = new EditorContext();
            try
            {
                await controller.GetDownload(
                    context,
                    new ExtractDownloadsOptions(),
                    "not_a_guid_without_dashes");
                throw new XunitException("Expected a validation exception but did not receive any");
            }
            catch (ValidationException)
            {
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task When_uploading_an_extract_that_is_not_a_zip(bool isFeatureCompare)
        {
            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
            var context = new EditorContext();

            var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
            {
                Headers = new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
                })
            };

            try
            {
                await controller.PostUpload(
                    context,
                    "not_a_guid_without_dashes",
                    formFile,
                    isFeatureCompare);
                throw new XunitException("Expected a validation exception but did not receive any");
            }
            catch (ValidationException)
            {
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task When_uploading_an_extract_that_is_a_zip(bool isFeatureCompare)
        {
            var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
            var downloadExtractRequestBodyValidator = new DownloadExtractRequestBodyValidator(wktReader, new NullLogger<DownloadExtractRequestBodyValidator>());
            var downloadExtractByContourRequestBodyValidator = new DownloadExtractByContourRequestBodyValidator(wktReader, new NullLogger<DownloadExtractByContourRequestBodyValidator>());
            var downloadExtractByNisCodeRequestBodyValidator = new DownloadExtractByNisCodeRequestBodyValidator(_editorContext);

            var controller = new ExtractsController(
                SystemClock.Instance,
                Dispatch.Using(_resolver),
                _downloadClient,
                _uploadClient,
                wktReader,
                downloadExtractRequestBodyValidator,
                downloadExtractByContourRequestBodyValidator,
                downloadExtractByNisCodeRequestBodyValidator,
                _editorContext)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };

            var context = new EditorContext();
            var client = new RoadNetworkUploadsBlobClient(new MemoryBlobClient());
            var store = new InMemoryStreamStore();

            try
            {
                using (var sourceStream = new MemoryStream())
                {
                    using (var embeddedStream =
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

                    var result = await controller.PostUpload(
                        context,
                        "not_a_guid_without_dashes",
                        formFile,
                        isFeatureCompare);

                    Assert.IsType<OkResult>(result);

                    var page = await store.ReadAllForwards(Position.Start, 1, true);
                    var message = Assert.Single(page.Messages);
                    Assert.Equal(nameof(Messages.RoadNetworkExtractChangesArchiveUploaded), message.Type);
                    var uploaded =
                        JsonConvert.DeserializeObject<Messages.RoadNetworkExtractChangesArchiveUploaded>(
                            await message.GetJsonData());

                    Assert.True(await client.BlobExistsAsync(new BlobName(uploaded.ArchiveId)));
                    var blob = await client.GetBlobAsync(new BlobName(uploaded.ArchiveId));
                    using (var openStream = await blob.OpenAsync())
                    {
                        var resultStream = new MemoryStream();
                        openStream.CopyTo(resultStream);
                        resultStream.Position = 0;
                        sourceStream.Position = 0;
                        Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
                    }
                }
            }
            catch (ValidationException)
            {
            }
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
    }
}
