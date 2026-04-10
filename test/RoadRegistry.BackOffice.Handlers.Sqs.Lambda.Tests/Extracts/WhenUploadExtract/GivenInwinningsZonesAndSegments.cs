namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadExtract;

using AutoFixture;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;
using AddRoadSegment = RoadRegistry.BackOffice.Uploads.AddRoadSegment;
using LineString = NetTopologySuite.Geometries.LineString;
using ModifyRoadSegment = RoadRegistry.BackOffice.Uploads.ModifyRoadSegment;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class GivenInwinningsZonesAndSegments : WhenUploadExtractTestBase
{
    public GivenInwinningsZonesAndSegments(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            CurrentDownloadId = downloadId,
            Description = ObjectProvider.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });

        var addRoadSegment = ObjectProvider.Create<AddRoadSegment>()
            .WithGeometry(new MultiLineString([new LineString([new Coordinate(0, 0), new Coordinate(1, 1)])]));
        var modifyRoadSegment1 = ObjectProvider.Create<ModifyRoadSegment>()
            .WithOriginalId(null);
        var modifyRoadSegment2 = ObjectProvider.Create<ModifyRoadSegment>()
            .WithOriginalId(ObjectProvider.Create<RoadSegmentId>());

        var translatedChanges = TranslatedChanges.Empty
            .AppendChange(addRoadSegment)
            .AppendChange(modifyRoadSegment1)
            .AppendChange(modifyRoadSegment2)
            ;

        ExtractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = ObjectProvider.Create<Guid>(),
            NisCode = ObjectProvider.Create<string>(),
            Operator = ObjectProvider.Create<string>(),
            Contour = addRoadSegment.Geometry.GetSingleLineString().StartPoint.Buffer(1),
            Completed = ObjectProvider.Create<bool>()
        });
        ExtractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = modifyRoadSegment1.Id,
            Completed = ObjectProvider.Create<bool>()
        });
        ExtractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = modifyRoadSegment2.Id,
            Completed = ObjectProvider.Create<bool>()
        });

        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request, translatedChanges: translatedChanges);

        // Assert
        VerifyThatTicketHasError(code: "ErrorRoadSegmentOverlapsWithInwinningszone", message: $"Het wegsegment met WS_OIDN {addRoadSegment.OriginalId ?? addRoadSegment.TemporaryId} valt (gedeeltelijk) binnen een gemeente die de inwinningsstatus 'locked' of 'compleet' heeft.");
        VerifyThatTicketHasError(code: "ErrorRoadSegmentIsInInwinning", message: $"Het wegsegment met WS_OIDN {modifyRoadSegment1.Id} heeft de inwinningsstatus 'locked' of 'compleet'.");
        VerifyThatTicketHasError(code: "ErrorRoadSegmentIsInInwinning", message: $"Het wegsegment met WS_OIDN {modifyRoadSegment2.OriginalId!.Value} heeft de inwinningsstatus 'locked' of 'compleet'.");
    }
}
