namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenCreateOutline.Abstractions.Fixtures;

using Api.RoadSegments;
using BackOffice.Extracts.Dbase.Organizations;
using Editor.Schema;
using FeatureToggles;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Tests.BackOffice.Scenarios;

public abstract class WhenCreateOutlineFixture : ControllerActionFixture<PostRoadSegmentOutlineParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    public readonly RoadNetworkTestData TestData = new();
    private IOrganizationRepository _organizationRepository;

    protected WhenCreateOutlineFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;

        TestData.CopyCustomizationsTo(ObjectProvider);

        _organizationRepository = new FakeOrganizationRepository();
    }

    public void CustomizeOrganizationRepository(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository.ThrowIfNull();
    }

    protected override async Task<IActionResult> GetResultAsync(PostRoadSegmentOutlineParameters request)
    {
        await _editorContext.Organizations.AddAsync(new OrganizationRecord
        {
            Id = 0,
            Code = "TEST",
            SortableCode = "TEST",
            DbaseSchemaVersion = WellKnownDbaseSchemaVersions.V2,
            DbaseRecord = Array.Empty<byte>()
        }, CancellationToken.None);

        await _editorContext.SaveChangesAsync(CancellationToken.None);

        var controller = new RoadSegmentsController(new FakeTicketingOptions(), _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var validator = new PostRoadSegmentOutlineParametersValidator(_organizationRepository);
        return await controller.CreateOutline(new UseRoadSegmentOutlineFeatureToggle(true), validator, request, CancellationToken.None);
    }
}
