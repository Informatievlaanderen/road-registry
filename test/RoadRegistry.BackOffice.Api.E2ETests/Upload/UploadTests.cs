namespace RoadRegistry.BackOffice.Api.E2ETests.Upload
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Extracts;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using RoadSegments;
    using Xunit;
    using Xunit.Abstractions;

    public class UploadTests : IClassFixture<ApiClientTestFixture>
    {
        private const string ExtractDescriptionPrefix = "E2ETest_";

        protected ApiClientTestFixture Fixture { get; }
        protected ITestOutputHelper OutputHelper { get; }

        public UploadTests(ApiClientTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            OutputHelper = outputHelper;
        }

        [Fact]
        public async Task CanRequestExtractAndDoSuccessfulUpload()
        {
            var publicApiClient = await Fixture.CreatePublicApiClient(new[] { Scopes.DvWrGeschetsteWegBeheer });
            if (publicApiClient is null)
            {
                return;
            }

            var roadSegmentGeometryAsGml = "<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\" srsName=\"urn:ogc:def:crs:EPSG::31370\" srsDimension=\"2\"><gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList></gml:LineString>";
            await publicApiClient.CreateOutlineRoadSegment(new PostRoadSegmentOutlineParameters
            {
                MiddellijnGeometrie = roadSegmentGeometryAsGml,
                Wegsegmentstatus = "in gebruik",
                MorfologischeWegklasse = "dienstweg",
                Toegangsbeperking = "openbare weg",
                Wegbeheerder = "71022",
                Wegverharding = "weg met vaste verharding",
                Wegbreedte = "2",
                AantalRijstroken = new RoadSegmentLaneParameters
                {
                    Aantal = "1",
                    Richting = "gelijklopend met de digitalisatiezin"
                }
            }, CancellationToken.None);

            var extractGeometry = GeometryTranslator.ParseGmlLineString(roadSegmentGeometryAsGml).Buffer(1).AsText();

            var backOfficeApiClient = await Fixture.CreateBackOfficeApiClient(new[] { Scopes.DvWrIngemetenWegBeheer });
            if (backOfficeApiClient is null)
            {
                return;
            }

            var extractRequest = await backOfficeApiClient.RequestDownloadExtract(new DownloadExtractRequestBody(
                extractGeometry,
                $"{ExtractDescriptionPrefix}{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
                false
            ), CancellationToken.None);
        }
    }
}
