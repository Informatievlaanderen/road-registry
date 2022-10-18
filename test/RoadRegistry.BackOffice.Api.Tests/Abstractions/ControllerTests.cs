namespace RoadRegistry.BackOffice.Api.Tests.Abstractions;

using BackOffice.Extracts;
using BackOffice.Uploads;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

public abstract class ControllerTests<TController> where TController : ControllerBase
{
    protected ControllerTests(IMediator mediator, IStreamStore streamStore, RoadNetworkUploadsBlobClient uploadClient, RoadNetworkExtractUploadsBlobClient extractUploadClient, RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
    {
        Controller = (TController)Activator.CreateInstance(typeof(TController), mediator);
        Controller!.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        Mediator = mediator;
        StreamStore = streamStore;
        UploadBlobClient = uploadClient;
        ExtractUploadBlobClient = extractUploadClient;
        FeatureCompareBlobClient = featureCompareBlobClient;
    }

    protected TController Controller { get; init; }
    protected RoadNetworkExtractUploadsBlobClient ExtractUploadBlobClient { get; }
    protected RoadNetworkFeatureCompareBlobClient FeatureCompareBlobClient { get; }

    protected IMediator Mediator { get; }
    protected IStreamStore StreamStore { get; }
    protected RoadNetworkUploadsBlobClient UploadBlobClient { get; }
}
