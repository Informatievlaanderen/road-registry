namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using Api.Uploads;
using BackOffice.Extracts;
using BackOffice.Uploads;
using MediatR;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using SqlStreamStore;

public partial class UploadControllerTests : ControllerTests<UploadController>
{
    public UploadControllerTests(
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient,
        RoadNetworkFeatureCompareBlobClient featureCompareBlobClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient, featureCompareBlobClient)
    {
    }
}
