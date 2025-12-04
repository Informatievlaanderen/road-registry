namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Abstractions.Fixtures;

using Api.Infrastructure.Controllers;
using Api.RoadSegments;
using BackOffice.Extracts.Dbase.Organizations;
using Editor.Schema;
using Editor.Schema.Organizations;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Extensions;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework.Projections;

public abstract class WhenCreateOutlineFixture : ControllerActionFixture<PostRoadSegmentOutlineParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    public readonly RoadNetworkTestData TestData = new();
    private IOrganizationCache _organizationCache;

    protected WhenCreateOutlineFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;

        TestData.CopyCustomizationsTo(ObjectProvider);

        _organizationCache = new FakeOrganizationCache();
    }

    public void CustomizeOrganizationCache(IOrganizationCache organizationCache)
    {
        _organizationCache = organizationCache.ThrowIfNull();
    }

    protected override async Task<IActionResult> GetResultAsync(PostRoadSegmentOutlineParameters request)
    {
        await _editorContext.AddOrganization("TEST");

        await _editorContext.SaveChangesAsync(CancellationToken.None);

        var controller = new RoadSegmentsController(new BackofficeApiControllerContext(new FakeTicketingOptions(), new HttpContextAccessor()), _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var validator = new PostRoadSegmentOutlineParametersValidator(_organizationCache);
        return await controller.CreateOutline(validator, request, CancellationToken.None);
    }
}
