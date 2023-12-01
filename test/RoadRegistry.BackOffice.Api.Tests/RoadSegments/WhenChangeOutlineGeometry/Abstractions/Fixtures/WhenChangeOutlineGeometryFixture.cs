namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeOutlineGeometry.Abstractions.Fixtures;

using Api.RoadSegments;
using AutoFixture;
using BackOffice.Extracts.Dbase.Organizations;
using Editor.Schema;
using Infrastructure;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts;
using RoadRegistry.Tests.BackOffice.Scenarios;

public abstract class WhenChangeOutlineGeometryFixture : ControllerActionFixture<PostChangeOutlineGeometryParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    private readonly IRoadSegmentRepository _roadSegmentRepository;

    protected WhenChangeOutlineGeometryFixture(IMediator mediator, EditorContext editorContext, IRoadSegmentRepository roadSegmentRepository)
    {
        _mediator = mediator;
        _editorContext = editorContext;
        _roadSegmentRepository = roadSegmentRepository;

        TestData = new RoadNetworkTestData(fixture => { fixture.CustomizeRoadSegmentOutlineGeometryDrawMethod(); }).CopyCustomizationsTo(ObjectProvider);
    }

    public RoadNetworkTestData TestData { get; protected set; }

    protected override async Task<IActionResult> GetResultAsync(PostChangeOutlineGeometryParameters request)
    {
        var controller = new RoadSegmentsController(new FakeTicketingOptions(), _mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var idValidator = new RoadSegmentIdValidator();
        var validator = new PostChangeOutlineGeometryParametersValidator();
        return await controller.ChangeOutlineGeometry(idValidator, validator, _roadSegmentRepository, request, TestData.Segment1Added.Id, CancellationToken.None);
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
