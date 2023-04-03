namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.RoadSegments;
using RoadRegistry.BackOffice.Api.RoadSegments.Parameters;
using RoadRegistry.BackOffice.Api.Tests.Framework;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Hosts.Infrastructure.Options;

public abstract class WhenDeleteOutlineFixture : ControllerActionFixture<PostDeleteOutlineParameters>
{
    private readonly IMediator _mediator;
    
    protected WhenDeleteOutlineFixture(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    protected override async Task<IActionResult> GetResultAsync(PostDeleteOutlineParameters parameters)
    {
        var controller = new RoadSegmentsController(new TicketingOptions { InternalBaseUrl = "http://internal/tickets", PublicBaseUrl = "http://public/tickets" }, _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.PostDeleteOutline(
            new UseRoadSegmentOutlineDeleteFeatureToggle(true),
            new PostDeleteOutlineParametersValidator(),
            parameters.WegsegmentId,
            CancellationToken.None
        );
    }
}
