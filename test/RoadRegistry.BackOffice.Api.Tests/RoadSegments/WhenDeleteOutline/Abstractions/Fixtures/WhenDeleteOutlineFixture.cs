namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenDeleteOutline.Abstractions.Fixtures;

using Api.RoadSegments;
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

public abstract class WhenDeleteOutlineFixture : ControllerActionFixture<int>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    private readonly IRoadSegmentRepository _roadSegmentRepository;

    protected WhenDeleteOutlineFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
    {
        _mediator = mediator;
        _editorContext = editorContext;
        _roadSegmentRepository = roadSegmentRepository;

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
            new RoadSegmentIdValidator(),
            _roadSegmentRepository,
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
