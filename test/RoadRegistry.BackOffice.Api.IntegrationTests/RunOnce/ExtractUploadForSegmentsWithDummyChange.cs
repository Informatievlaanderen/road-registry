namespace RoadRegistry.BackOffice.Api.IntegrationTests.RunOnce;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Api.Extracts;
using BackOffice.Extracts;
using BackOffice.Uploads;
using BackOffice.Uploads.Dbase.BeforeFeatureCompare.V2.Schema;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
using Extracts;
using FeatureCompare.V2;
using FeatureCompare.V2.Readers;
using NetTopologySuite.Geometries;
using ShapeFile.V2;
using Tests;
using Tests.BackOffice.Scenarios;
using Xunit;
using Xunit.Abstractions;
using Point = NetTopologySuite.Geometries.Point;
using ShapeType = NetTopologySuite.IO.Esri.ShapeType;

public class ExtractUploadForSegmentsWithDummyChange : IClassFixture<ApiClientTestFixture>
{
    private readonly ApiClientTestFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public ExtractUploadForSegmentsWithDummyChange(ApiClientTestFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task RunOnce()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var segmentIds = new RoadSegmentId[]
        {
            //1087278,
            //1134688,
        };

        var cancellationToken = CancellationToken.None;

        foreach (var segmentId in segmentIds)
        {
            _testOutputHelper.WriteLine($"Processing segment {segmentId}...");
            try
            {
                await ModifyRoadSegment(segmentId, cancellationToken);
            }
            catch (Exception ex)
            {
                _testOutputHelper.WriteLine($"Error processing segment: {ex}");
                throw;
            }
        }
    }

    private async Task ModifyRoadSegment(RoadSegmentId segmentId, CancellationToken cancellationToken)
    {
        using var apiClient = await _fixture.CreateApiClient([Scopes.DvWrIngemetenWegBeheer]);
        if (apiClient is null)
        {
            return;
        }

        var autoFixture = new RoadNetworkTestData().ObjectProvider;
        var reader = new RoadSegmentFeatureCompareFeatureReader(FileEncoding.WindowsAnsi);
        var writer = new ShapeFileRecordWriter(FileEncoding.WindowsAnsi);

        var segment = await apiClient.GetRoadSegment(segmentId, cancellationToken);
        var firstPoint = new Point(segment.MiddellijnGeometrie.Coordinates[0][0][0], segment.MiddellijnGeometrie.Coordinates[0][0][1]);

        var extractRequest = await apiClient.RequestDownloadExtractByContour(new DownloadExtractByContourRequestBody(
            0,
            firstPoint.Buffer(1).Envelope.AsText(),
            $"{DateTime.Today:yyyy-MM-dd} opkuis wegsegment {segmentId}",
            false), cancellationToken);

        using var extractArchiveStream = new MemoryStream();
        using (var extractArchive = await apiClient.DownloadExtractAndWait(extractRequest.DownloadId, extractArchiveStream, cancellationToken))
        {
            var fileName = ExtractFileName.Wegsegment;
            var featureType = FeatureType.Extract;

            var extractSegments = reader.Read(extractArchive, featureType, new ZipArchiveFeatureReaderContext(ZipArchiveMetadata.Empty));

            var records = extractSegments.Item1
                .Select(x =>
                {
                    try
                    {
                        var segment = x.Attributes;

                        var dbfRecord = new RoadSegmentDbaseRecord
                        {
                            WS_OIDN = { Value = segment.Id },
                            LSTRNMID = { Value = segment.LeftStreetNameId },
                            RSTRNMID = { Value = segment.RightStreetNameId },
                            B_WK_OIDN = { Value = segment.StartNodeId },
                            E_WK_OIDN = { Value = segment.EndNodeId },
                            BEHEER = { Value = segment.MaintenanceAuthority },
                            WEGCAT = { Value = segment.Category.Translation.Identifier },
                            MORF = { Value = segment.Morphology.Translation.Identifier },
                            METHODE = { Value = segment.Method.Translation.Identifier },
                            STATUS = { Value = segment.Status.Translation.Identifier },
                            TGBEP = { Value = segment.AccessRestriction.Translation.Identifier },
                            // CATEGORIE = {  },
                            // BEHEERDER = {  },
                            // MORFOLOGIE = {  },
                        };

                        if (dbfRecord.WS_OIDN.Value == segmentId)
                        {
                            dbfRecord.MORF.Value = autoFixture.CreateWhichIsDifferentThan(segment.Morphology).Translation.Identifier;
                        }

                        return ((DbaseRecord)dbfRecord, (Geometry)x.Attributes.Geometry);
                    }
                    catch (Exception ex)
                    {
                        _testOutputHelper.WriteLine($"Error processing segment {x.Attributes.Id}: {ex}");
                        throw;
                    }
                })
                .ToList();

            DeleteShapeEntries(extractArchive, fileName, featureType);

            await writer.WriteToArchive(
                extractArchive,
                fileName,
                featureType,
                ShapeType.PolyLine,
                RoadSegmentDbaseRecord.Schema,
                records,
                cancellationToken);
        }

        await using var file = File.Create($"wegsegment-{segmentId}.zip");
        extractArchiveStream.Position = 0;
        await extractArchiveStream.CopyToAsync(file, cancellationToken);
        //await apiClient.Upload(extractArchiveStream, cancellationToken);
    }

    private static void DeleteShapeEntries(ZipArchive extractArchive, ExtractFileName fileName, FeatureType featureType)
    {
        var entries = new[]
        {
            extractArchive.FindEntry(fileName.ToShapeFileName(featureType)),
            extractArchive.FindEntry(fileName.ToDbaseFileName(featureType)),
            extractArchive.FindEntry(fileName.ToShapeIndexFileName(featureType)),
            extractArchive.FindEntry(fileName.ToProjectionFileName(featureType)),
            extractArchive.FindEntry(fileName.ToCpgFileName(featureType)),
        };
        foreach (var entry in entries)
        {
            entry.Delete();
        }
    }
}
