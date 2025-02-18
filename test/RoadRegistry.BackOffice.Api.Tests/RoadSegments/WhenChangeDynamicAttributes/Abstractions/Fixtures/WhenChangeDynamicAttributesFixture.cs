namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions.Fixtures;

using Api.RoadSegments;
using Api.RoadSegments.ChangeDynamicAttributes;
using AutoFixture;
using Editor.Schema;
using Editor.Schema.Organizations;
using Infrastructure;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadRegistry.Tests.Framework.Projections;

public abstract class WhenChangeDynamicAttributesFixture : ControllerActionFixture<ChangeRoadSegmentsDynamicAttributesParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    public readonly RoadNetworkTestData TestData = new();

    protected WhenChangeDynamicAttributesFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;

        TestData.CopyCustomizationsTo(ObjectProvider);

        ObjectProvider.CustomizeRoadSegmentOutlineLaneCount();
        ObjectProvider.CustomizeRoadSegmentLaneDirection();
        ObjectProvider.CustomizeRoadSegmentSurfaceType();
        ObjectProvider.CustomizeRoadSegmentOutlineWidth();
    }

    protected override async Task<IActionResult> GetResultAsync(ChangeRoadSegmentsDynamicAttributesParameters parameters)
    {
        var controller = new RoadSegmentsController(new FakeTicketingOptions(), _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.ChangeDynamicAttributes(
            parameters,
            new ChangeRoadSegmentsDynamicAttributesParametersValidator(_editorContext),
            CancellationToken.None
        );
    }

    protected override async Task SetupAsync()
    {
        await _editorContext.AddOrganization(TestData.ChangedByOrganization, TestData.ChangedByOrganizationName);

        var message = ObjectProvider
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(TestData.Segment1Added);

        await _editorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, message.When));
        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }
}
