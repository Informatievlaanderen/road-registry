namespace RoadRegistry.BackOffice.Api.Tests.RoadSegments.WhenChangeAttributes.Abstractions.Fixtures;

using System.Text;
using Api.RoadSegments;
using Api.RoadSegments.ChangeAttributes;
using AutoFixture;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.RoadSegments;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using Editor.Schema;
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
using RoadRegistry.Tests.BackOffice.Scenarios;

public abstract class WhenChangeAttributesFixture : ControllerActionFixture<ChangeRoadSegmentAttributesParameters>
{
    private readonly EditorContext _editorContext;
    private readonly IMediator _mediator;
    public readonly RoadNetworkTestData TestData = new();

    protected WhenChangeAttributesFixture(IMediator mediator, EditorContext editorContext)
    {
        _mediator = mediator;
        _editorContext = editorContext;

        TestData.CopyCustomizationsTo(ObjectProvider);

        ObjectProvider.CustomizeRoadSegmentOutlineMorphology();
        ObjectProvider.CustomizeRoadSegmentOutlineStatus();
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
            new ChangeRoadSegmentAttributesParametersWrapperValidator(_editorContext),
            CancellationToken.None
        );
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

        var segment = TestData.Segment1Added;
        var geometry = GeometryTranslator.Translate(segment.Geometry);
        var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

        await _editorContext.RoadSegments.AddAsync(new RoadSegmentRecord
        {
            Id = segment.Id,
            StartNodeId = segment.StartNodeId,
            EndNodeId = segment.EndNodeId,
            ShapeRecordContent = polyLineMShapeContent.ToBytes(new RecyclableMemoryStreamManager(), Encoding.UTF8),
            ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
            BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
            Geometry = geometry,
            DbaseRecord = new RoadSegmentDbaseRecord
            {
                WS_OIDN = { Value = segment.Id },
                WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                B_WK_OIDN = { Value = segment.StartNodeId },
                E_WK_OIDN = { Value = segment.EndNodeId },
                STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                LSTRNM = { Value = null },
                RSTRNMID = { Value = segment.RightSide.StreetNameId },
                RSTRNM = { Value = null },
                BEHEER = { Value = segment.MaintenanceAuthority.Code },
                LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier },
                LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name },
                OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                BEGINORG = { Value = TestData.ChangedByOrganization },
                LBLBGNORG = { Value = TestData.ChangedByOrganization },
                TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
            }.ToBytes(new RecyclableMemoryStreamManager(), Encoding.UTF8),
            LastEventHash = segment.GetHash()
        });

        await _editorContext.SaveChangesAsync(CancellationToken.None);
    }
}
