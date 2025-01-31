namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using System.Diagnostics;
using Abstractions.RoadSegments;
using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Editor.Schema;
using Editor.Schema.Extensions;
using FluentValidation;
using Framework;
using MediatR;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Product.Schema;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using Reason = Reason;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class CorrectRoadSegmentOrganizationNamesRequestHandler
    : IRequestHandler<CorrectRoadSegmentOrganizationNamesRequest, CorrectRoadSegmentOrganizationNamesResponse>
{
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly EditorContext _editorContext;
    private readonly ProductContext _productContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly ILogger<CorrectRoadSegmentOrganizationNamesRequestHandler> _logger;

    public CorrectRoadSegmentOrganizationNamesRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadRegistryContext roadRegistryContext,
        EditorContext editorContext,
        ProductContext productContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        ILogger<CorrectRoadSegmentOrganizationNamesRequestHandler> logger)
    {
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _roadRegistryContext = roadRegistryContext;
        _editorContext = editorContext;
        _productContext = productContext;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _logger = logger;
    }

    public async Task<CorrectRoadSegmentOrganizationNamesResponse> Handle(CorrectRoadSegmentOrganizationNamesRequest request, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(OrganizationId.DigitaalVlaanderen)
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Corrigeer wegsegmenten organisatie namen"));

        var invalidRoadSegmentIds = await GetInvalidRoadSegmentIds(cancellationToken);
        if (!invalidRoadSegmentIds.Any())
        {
            return new CorrectRoadSegmentOrganizationNamesResponse(0);
        }

        var network = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);

        var roadSegments = network.FindRoadSegments(invalidRoadSegmentIds.Select(x => new RoadSegmentId(x)));

        var recordNumber = RecordNumber.Initial;
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        foreach (var roadSegment in roadSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var modifyRoadSegment = new ModifyRoadSegment(
                recordNumber,
                roadSegment.Id,
                roadSegment.Start,
                roadSegment.End,
                roadSegment.AttributeHash.OrganizationId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.AttributeHash.Morphology,
                roadSegment.AttributeHash.Status,
                roadSegment.AttributeHash.Category,
                roadSegment.AttributeHash.AccessRestriction,
                roadSegment.AttributeHash.LeftStreetNameId,
                roadSegment.AttributeHash.RightStreetNameId
            ).WithGeometry(roadSegment.Geometry);

            foreach (var lane in roadSegment.Lanes)
            {
                modifyRoadSegment = modifyRoadSegment.WithLane(new RoadSegmentLaneAttribute(attributeIdProvider.Next(), lane.Count, lane.Direction, lane.From, lane.To));
            }
            foreach (var surface in roadSegment.Surfaces)
            {
                modifyRoadSegment = modifyRoadSegment.WithSurface(new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), surface.Type, surface.From, surface.To));
            }
            foreach (var width in roadSegment.Widths)
            {
                modifyRoadSegment = modifyRoadSegment.WithWidth(new RoadSegmentWidthAttribute(attributeIdProvider.Next(), width.Width, width.From, width.To));
            }

            translatedChanges = translatedChanges.AppendChange(modifyRoadSegment);

            recordNumber = recordNumber.Next();
        }

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();

        var changeRoadNetwork = new ChangeRoadNetwork
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(Guid.NewGuid())),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, cancellationToken);

        await _roadNetworkCommandQueue.WriteAsync(new Command(changeRoadNetwork), cancellationToken);

        return new CorrectRoadSegmentOrganizationNamesResponse(roadSegments.Count);
    }

    private async Task<List<int>> GetInvalidRoadSegmentIds(CancellationToken cancellationToken)
    {
        var roadSegmentIds = new List<int>();
        const int pageSize = 5000;

        {
            var pageIndex = 0;
            while (await FillInvalidRoadSegmentIdsFromEditorContext(pageIndex++, pageSize, roadSegmentIds, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        {
            var pageIndex = 0;
            while (await FillInvalidRoadSegmentIdsFromProductContext(pageIndex++, pageSize, roadSegmentIds, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        return roadSegmentIds.Distinct().ToList();
    }

    private async Task<bool> FillInvalidRoadSegmentIdsFromEditorContext(int pageIndex, int pageSize, List<int> roadSegmentIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(_editorContext.RoadSegments), pageIndex, pageSize);
        var roadSegments = await _editorContext.RoadSegments
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(_editorContext.RoadSegments), sw.ElapsedMilliseconds);

        sw.Restart();

        _logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(_editorContext.RoadSegments), pageIndex, pageSize);

        roadSegmentIds.AddRange(roadSegments
            .Where(x => string.IsNullOrEmpty(x.MaintainerName))
            .Select(x => x.Id));
        _logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(_editorContext.RoadSegments), sw.ElapsedMilliseconds);

        return roadSegments.Any();
    }

    private async Task<bool> FillInvalidRoadSegmentIdsFromProductContext(int pageIndex, int pageSize, List<int> roadSegmentIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for {EntityName} from ProductContext (Page {PageIndex}, Size {PageSize})", nameof(_productContext.RoadSegments), pageIndex, pageSize);
        var roadSegments = await _productContext.RoadSegments
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Read finished for {EntityName} from ProductContext in {StopwatchElapsedMilliseconds}ms", nameof(_productContext.RoadSegments), sw.ElapsedMilliseconds);

        sw.Restart();

        _logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from ProductContext (Page {PageIndex}, Size {PageSize})", nameof(_productContext.RoadSegments), pageIndex, pageSize);

        roadSegmentIds.AddRange(roadSegments
            .Select(x => new RoadSegmentDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .Where(x => string.IsNullOrEmpty(x.LBLBEHEER.GetValue()))
            .Select(x => x.WS_OIDN.Value));
        _logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from ProductContext in {StopwatchElapsedMilliseconds}ms", nameof(_productContext.RoadSegments), sw.ElapsedMilliseconds);

        return roadSegments.Any();
    }
}
