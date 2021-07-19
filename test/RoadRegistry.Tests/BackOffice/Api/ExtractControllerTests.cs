namespace RoadRegistry.BackOffice.Api
{
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice.Extracts;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Extracts;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging.Abstractions;
    using NetTopologySuite;
    using NetTopologySuite.IO;
    using NodaTime;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Sdk;

    public class ExtractControllerTests
    {
        private readonly Fixture _fixture;
        private readonly CommandHandlerResolver _resolver;

        public ExtractControllerTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeExternalExtractRequestId();
            _fixture.CustomizeRoadNetworkExtractGeometry();
            _resolver = Resolve.WhenEqualToMessage(
                new RoadNetworkExtractCommandModule(
                    new InMemoryStreamStore(),
                    new FakeRoadNetworkSnapshotReader(),
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
            var validator =
                new DownloadExtractRequestBodyValidator(wktReader,
                    new NullLogger<DownloadExtractRequestBodyValidator>());
            var controller = new ExtractsController(SystemClock.Instance, Dispatch.Using(_resolver), new ExtractDownloadsBlobClient(new MemoryBlobClient()), wktReader, validator)
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
            var validator =
                new DownloadExtractRequestBodyValidator(wktReader,
                    new NullLogger<DownloadExtractRequestBodyValidator>());
            var controller = new ExtractsController(SystemClock.Instance,Dispatch.Using(_resolver), new ExtractDownloadsBlobClient(new MemoryBlobClient()), wktReader, validator)
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
            var validator =
                new DownloadExtractRequestBodyValidator(wktReader,
                    new NullLogger<DownloadExtractRequestBodyValidator>());
            var controller = new ExtractsController(SystemClock.Instance,Dispatch.Using(_resolver), new ExtractDownloadsBlobClient(new MemoryBlobClient()), wktReader, validator)
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
            var validator =
                new DownloadExtractRequestBodyValidator(wktReader,
                    new NullLogger<DownloadExtractRequestBodyValidator>());
            var controller = new ExtractsController(SystemClock.Instance,Dispatch.Using(_resolver), new ExtractDownloadsBlobClient(new MemoryBlobClient()), wktReader, validator)
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
    }
}
