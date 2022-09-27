namespace RoadRegistry.BackOffice.Api.Tests.Abstractions;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using SqlStreamStore;
using System;
using Editor.Schema;
using Framework.Containers;

public abstract class ControllerTests<TController> where TController : ControllerBase
{
    protected ControllerTests(IMediator mediator, IStreamStore streamStore, RoadNetworkUploadsBlobClient uploadClient, RoadNetworkExtractUploadsBlobClient extractUploadClient)
    {
        Controller = (TController)Activator.CreateInstance(typeof(TController), mediator);
        Controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        Mediator = mediator;
        StreamStore = streamStore;
        UploadBlobClient = uploadClient;
        ExtractUploadClient = extractUploadClient;
    }

    protected IMediator Mediator { get; }
    protected IStreamStore StreamStore { get; }
    public RoadNetworkUploadsBlobClient UploadBlobClient { get; }
    protected RoadNetworkExtractUploadsBlobClient ExtractUploadClient { get; }

    protected TController Controller { get; init; }
}
