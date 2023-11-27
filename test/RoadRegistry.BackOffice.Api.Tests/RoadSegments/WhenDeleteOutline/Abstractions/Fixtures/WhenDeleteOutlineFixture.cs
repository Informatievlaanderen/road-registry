namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;

using System.Text;
using Api.RoadSegments;
using AutoFixture;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.RoadSegments;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using Editor.Schema;
using Editor.Schema.Extensions;
using Editor.Schema.RoadSegments;
using FeatureToggles;
using Hosts.Infrastructure.Options;
using Infrastructure;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;

public abstract class WhenDeleteOutlineFixture : ControllerActionFixture<int>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;

    protected WhenDeleteOutlineFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;

        TestData = new RoadNetworkTestData(fixture => { fixture.CustomizeRoadSegmentOutlineGeometryDrawMethod(); }).CopyCustomizationsTo(ObjectProvider);
    }

    public RoadNetworkTestData TestData { get; protected set; }

    protected override int CreateRequest()
    {
        return TestData.Segment1Added.Id;
    }

    protected override async Task<IActionResult> GetResultAsync(int roadSegmentId)
    {
        var controller = new RoadSegmentsController(new FakeTicketingOptions(), _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        return await controller.DeleteOutline(
            new UseRoadSegmentOutlineDeleteFeatureToggle(true),
            new RoadSegmentOutlinedIdValidator(_editorContext, new RecyclableMemoryStreamManager(), FileEncoding.UTF8),
            roadSegmentId,
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
