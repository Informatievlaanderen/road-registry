namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using Api.Uploads;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Editor.Schema;
using Editor.Schema.Extracts;
using Infrastructure;
using MediatR;
using NetTopologySuite.Geometries;
using SqlStreamStore;
using Xunit.Abstractions;

public partial class UploadControllerTests : ControllerTests<UploadController>, IAsyncLifetime
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly EditorContext _editorContext;

    public UploadControllerTests(
        ITestOutputHelper testOutputHelper,
        IMediator mediator,
        IStreamStore streamStore,
        EditorContext editorContext,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient,
        RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient, featureCompareBlobClient)
    {
        _testOutputHelper = testOutputHelper;
        _editorContext = editorContext;
    }

    public async Task InitializeAsync()
    {
        await _editorContext.ExtractRequests.AddRangeAsync(new ExtractRequestRecord[]
        {
            new ExtractRequestRecord {
                Contour = Geometry.DefaultFactory.CreateEmpty(Dimension.Unknown),
                Description = "Valid-Before",
                ExternalRequestId = "",
                DownloadId = DownloadId.Parse("d554de226e6842c597d392a50636fa45").ToGuid(),
                IsInformative = false,
                RequestedOn = DateTimeOffset.UtcNow
            },
            new ExtractRequestRecord {
                Contour = Geometry.DefaultFactory.CreateEmpty(Dimension.Unknown),
                Description = "Valid-After",
                ExternalRequestId = "",
                DownloadId = DownloadId.Parse("bc57a81104a3481f87920f008dece82b").ToGuid(),
                IsInformative = false,
                RequestedOn = DateTimeOffset.UtcNow
            }
        });

        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await _editorContext.DisposeAsync();
    }
}
