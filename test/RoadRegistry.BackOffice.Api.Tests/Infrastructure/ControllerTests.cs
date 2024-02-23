namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using BackOffice.Extracts;
using BackOffice.Uploads;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SqlStreamStore;

public abstract class ControllerTests<TController> : ControllerMinimalTests<TController>
    where TController : ControllerBase
{
    protected ControllerTests(
        TController controller,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(controller, mediator)
    {
        StreamStore = streamStore.ThrowIfNull();
        UploadBlobClient = uploadClient.ThrowIfNull();
        ExtractUploadBlobClient = extractUploadClient.ThrowIfNull();
    }

    protected IStreamStore StreamStore { get; }
    protected RoadNetworkExtractUploadsBlobClient ExtractUploadBlobClient { get; }
    protected RoadNetworkUploadsBlobClient UploadBlobClient { get; }
}
