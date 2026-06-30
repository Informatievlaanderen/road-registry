namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutlineV2.Abstractions.Fixtures;

using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Api.Infrastructure.Controllers;
using RoadRegistry.BackOffice.Api.V2.RoadSegments;
using RoadRegistry.Tests;

public abstract class WhenCreateOutlineV2Fixture : ControllerActionFixture<CreateOutlinedRoadSegmentV2Parameters>
{
    private readonly IMediator _mediator;

    protected WhenCreateOutlineV2Fixture(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override async Task<IActionResult> GetResultAsync(CreateOutlinedRoadSegmentV2Parameters request)
    {
        var controller = new RoadSegmentsController(
            new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()),
            _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var validator = new CreateOutlinedRoadSegmentV2ParametersValidator(new FakeOrganizationCache());
        return await controller.CreateOutlinedRoadSegmentV2(validator, request, CancellationToken.None);
    }
}
