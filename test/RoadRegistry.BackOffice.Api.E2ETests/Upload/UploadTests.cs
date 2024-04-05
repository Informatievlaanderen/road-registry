namespace RoadRegistry.BackOffice.Api.E2ETests.Upload
{
    using System;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Extracts;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Extensions;
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
            //var publicApiClient = await Fixture.CreatePublicApiClient(new[] { Scopes.DvWrGeschetsteWegBeheer });
            //if (publicApiClient is null)
            //{
            //    return;
            //}

            //var roadSegmentGeometryAsGml = "<gml:LineString xmlns:gml=\"http://www.opengis.net/gml/3.2\" srsName=\"urn:ogc:def:crs:EPSG::31370\" srsDimension=\"2\"><gml:posList>217368.75 181577.016 217400.11 181499.516</gml:posList></gml:LineString>";
            //var roadSegmentId = await publicApiClient.CreateOutlineRoadSegment(new PostRoadSegmentOutlineParameters
            //{
            //    MiddellijnGeometrie = roadSegmentGeometryAsGml,
            //    Wegsegmentstatus = "in gebruik",
            //    MorfologischeWegklasse = "dienstweg",
            //    Toegangsbeperking = "openbare weg",
            //    Wegbeheerder = "AGIV",
            //    Wegverharding = "weg met vaste verharding",
            //    Wegbreedte = "2",
            //    AantalRijstroken = new RoadSegmentLaneParameters
            //    {
            //        Aantal = "1",
            //        Richting = "gelijklopend met de digitalisatiezin"
            //    }
            //}, CancellationToken.None);
            
            var backOfficeApiClient = await Fixture.CreateBackOfficeApiClient(new[] { Scopes.DvWrIngemetenWegBeheer });
            if (backOfficeApiClient is null)
            {
                return;
            }

            var downloadId = "cd31606218e14ad980734df2a7c46b19";
            //var extractRequest = await backOfficeApiClient.RequestDownloadExtract(new DownloadExtractRequestBody(
            //    GeometryTranslator.ParseGmlLineString(roadSegmentGeometryAsGml).Buffer(1).AsText(),
            //    $"{ExtractDescriptionPrefix}{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
            //    false
            //), CancellationToken.None);
            //downloadId = extractRequest.DownloadId;

            await using var extractArchiveStream = await backOfficeApiClient.DownloadExtract(downloadId, CancellationToken.None);
            var writeableExtractArchiveStream = await extractArchiveStream.CopyToNewMemoryStream(CancellationToken.None);
            using var zipArchive = new ZipArchive(writeableExtractArchiveStream, ZipArchiveMode.Update);

            var extractEntries = zipArchive.Entries.Where(x => x.Name.StartsWith('e')).ToArray();

            foreach (var extractEntry in extractEntries)
            {
                var changeEntry = zipArchive.CreateEntry(extractEntry.Name[1..]);

                await using var newEntryStream = changeEntry.Open();
                await using var entryStream = extractEntry.Open();
                await entryStream.CopyToAsync(newEntryStream, CancellationToken.None);
            }
            //TODO-rik read roadsegments file and edit status of the one which was created
            //check ziparchivecleaner voor logica om te lezen en te updaten

            writeableExtractArchiveStream.Position = 0;
            await backOfficeApiClient.UploadExtract(writeableExtractArchiveStream.ToArray(), CancellationToken.None);

            //TODO-rik wait for change to be active in read endpoint


            //TODO-rik delete outlined roadsegment

            var a = true;
        }
    }
}
