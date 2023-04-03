namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Abstractions.Fixtures;

using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.RoadSegments;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using RoadRegistry.BackOffice.Api.Tests.Framework;
using RoadRegistry.BackOffice.Extracts.Dbase.Organizations;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Hosts.Infrastructure.Options;

public abstract class WhenCreateOutlineFixture : ControllerActionFixture<PostRoadSegmentOutlineParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;

    protected WhenCreateOutlineFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;
    }

    protected override async Task<IActionResult> GetResultAsync(PostRoadSegmentOutlineParameters request)
    {
        await _editorContext.Organizations.AddAsync(new OrganizationRecord
        {
            Id = 0,
            Code = "TEST",
            SortableCode = "TEST",
            DbaseRecord = Array.Empty<byte>()
        }, CancellationToken.None);

        await _editorContext.SaveChangesAsync(CancellationToken.None);

        var controller = new RoadSegmentsController(new TicketingOptions { InternalBaseUrl = "http://internal/tickets", PublicBaseUrl = "http://public/tickets" }, _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var validator = new PostRoadSegmentOutlineParametersValidator(_editorContext);
        return await controller.PostCreateOutline(new UseRoadSegmentOutlineFeatureToggle(true), validator, request, CancellationToken.None);
    }
}
