namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using BackOffice.Extracts;
using BackOffice.Uploads;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

public abstract class ControllerTests<TController> : ControllerMinimalTests<TController>
    where TController : ControllerBase
{
    protected ControllerTests(TController controller, IMediator mediator, IStreamStore streamStore, RoadNetworkUploadsBlobClient uploadClient, RoadNetworkExtractUploadsBlobClient extractUploadClient, RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
        : base(controller, mediator)
    {
        StreamStore = streamStore;
        UploadBlobClient = uploadClient;
        ExtractUploadBlobClient = extractUploadClient;
        FeatureCompareBlobClient = featureCompareBlobClient;
    }

    protected IStreamStore StreamStore { get; }
    protected RoadNetworkExtractUploadsBlobClient ExtractUploadBlobClient { get; }
    protected RoadNetworkFeatureCompareBlobClient FeatureCompareBlobClient { get; }
    protected RoadNetworkUploadsBlobClient UploadBlobClient { get; }
}
