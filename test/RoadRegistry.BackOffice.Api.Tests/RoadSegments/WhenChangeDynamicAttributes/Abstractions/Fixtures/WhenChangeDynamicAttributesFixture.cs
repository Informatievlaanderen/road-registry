namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeDynamicAttributes.Abstractions.Fixtures;

using System.Text;
using Api.RoadSegments.ChangeDynamicAttributes;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using RoadRegistry.BackOffice.Api.RoadSegments;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.BackOffice.Extracts.Dbase.Organizations;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Editor.Projections;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Editor.Schema.RoadSegments;
using RoadRegistry.Hosts.Infrastructure.Options;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;

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
        await _editorContext.Organizations.AddAsync(new OrganizationRecord
        {
            Code = TestData.ChangedByOrganization,
            SortableCode = TestData.ChangedByOrganization,
            DbaseSchemaVersion = WellKnownDbaseSchemaVersions.V2,
            DbaseRecord = Array.Empty<byte>()
        }, CancellationToken.None);

        var message = ObjectProvider
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(TestData.Segment1Added);

        await _editorContext.RoadSegments.AddAsync(TestData.Segment1Added.ToRoadSegmentRecord(TestData.ChangedByOrganization, message.When));
        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }
}
