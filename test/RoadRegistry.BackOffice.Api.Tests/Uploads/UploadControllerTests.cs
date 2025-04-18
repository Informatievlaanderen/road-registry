namespace RoadRegistry.BackOffice.Api.Tests.Uploads;

using Api.Uploads;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Infrastructure;
using MediatR;
using SqlStreamStore;

public partial class UploadControllerTests : ControllerTests<UploadController>
{
    public UploadControllerTests(
        UploadController controller,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(controller, mediator, streamStore, uploadClient, extractUploadClient)
    {
    }
}
