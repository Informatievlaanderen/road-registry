namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

using Api.RoadSegments;
using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using BackOffice.Extracts.Dbase.Organizations;
using Editor.Schema;
using FeatureToggles;
using Infrastructure;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;

public abstract class WhenChangeAttributesFixture : ControllerActionFixture<ChangeRoadSegmentAttributesParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    public readonly RoadNetworkTestData TestData = new();

    private IOrganizationCache _organizationCache;

    protected WhenChangeAttributesFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;

        TestData.CopyCustomizationsTo(ObjectProvider);

        ObjectProvider.CustomizeRoadSegmentOutlineMorphology();
        ObjectProvider.CustomizeRoadSegmentOutlineStatus();
        _organizationCache = new FakeOrganizationCache();
    }

    protected override async Task<IActionResult> GetResultAsync(ChangeRoadSegmentAttributesParameters parameters)
    {
        var controller = new RoadSegmentsController(new FakeTicketingOptions(), _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.ChangeAttributes(
            new UseRoadSegmentChangeAttributesFeatureToggle(true),
            parameters,
            new ChangeRoadSegmentAttributesParametersValidator(),
            new ChangeRoadSegmentAttributesParametersWrapperValidator(_editorContext, _organizationCache),
            CancellationToken.None
        );
    }

    public void CustomizeOrganizationCache(IOrganizationCache organizationCache)
    {
        _organizationCache = organizationCache.ThrowIfNull();
    }

    protected override async Task SetupAsync()
    {
        await _editorContext.Organizations.AddAsync(new OrganizationRecord
        {
            Code = TestData.ChangedByOrganization,
            SortableCode = TestData.ChangedByOrganization,
            DbaseRecord = Array.Empty<byte>(),
            DbaseSchemaVersion = WellKnownDbaseSchemaVersions.V2
        }, CancellationToken.None);

        var message = ObjectProvider
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(TestData.Segment1Added);

        await _editorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, message.When));
        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }
}
