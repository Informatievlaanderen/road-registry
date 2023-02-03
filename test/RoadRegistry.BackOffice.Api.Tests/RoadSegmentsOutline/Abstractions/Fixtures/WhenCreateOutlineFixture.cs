namespace RoadRegistry.BackOffice.Api.Tests.RoadSegmentsOutline.Abstractions.Fixtures;

using Api.RoadSegmentsOutline;
using Dbase.Organizations;
using Editor.Schema;
using FeatureToggles;
using Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.RoadSegmentsOutline.Parameters;

public abstract class WhenCreateOutlineFixture : ApplicationFixture, IAsyncLifetime
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;

    protected WhenCreateOutlineFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;
    }

    public PostRoadSegmentOutlineParameters Parameters { get; private set; }

    public IActionResult Result { get; private set; }
    public Exception Exception { get; private set; }
    public bool HasException => Exception != null;

    public async Task InitializeAsync()
    {
        Parameters = CreateRequestParameters();

        await _editorContext.Organizations.AddAsync(new OrganizationRecord
        {
            Id = 0,
            Code = "TEST",
            SortableCode = "TEST",
            DbaseRecord = Array.Empty<byte>()
        }, CancellationToken.None);

        await _editorContext.SaveChangesAsync(CancellationToken.None);

        var controller = new RoadSegmentsOutlineController(new TicketingOptions { InternalBaseUrl = "http://internal/tickets", PublicBaseUrl = "http://public/tickets" }, _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        try
        {
            Result = await controller.PostCreateOutline(new UseRoadSegmentOutlineFeatureToggle(true), _editorContext, Parameters, CancellationToken.None);
            Exception = null;
        }
        catch (Exception ex)
        {
            Result = null;
            Exception = ex;
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected abstract PostRoadSegmentOutlineParameters CreateRequestParameters();
}
