namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Editor.Projections;
using RoadRegistry.Editor.Schema;
using System.Diagnostics;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using Reason = BackOffice.Reason;

public sealed class CorrectRoadSegmentVersionsRequestHandler : IRequestHandler<CorrectRoadSegmentVersionsRequest, CorrectRoadSegmentVersionsResponse>
{
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly ILogger<CorrectRoadSegmentVersionsRequestHandler> _logger;

    public CorrectRoadSegmentVersionsRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadRegistryContext roadRegistryContext,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        ILogger<CorrectRoadSegmentVersionsRequestHandler> logger)
    {
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _roadRegistryContext = roadRegistryContext;
        _editorContextFactory = editorContextFactory;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _logger = logger;
    }

    public async Task<CorrectRoadSegmentVersionsResponse> Handle(CorrectRoadSegmentVersionsRequest request, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId(Organisation.DigitaalVlaanderen.ToString()))
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Corrigeer wegsegmenten versies"));

        var roadSegmentIdsWithGeometryVersionZero = await GetRoadSegmentIdsWithInvalidVersions(cancellationToken);
        if (!roadSegmentIdsWithGeometryVersionZero.Any())
        {
            return new CorrectRoadSegmentVersionsResponse(0);
        }

        var network = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);

        var roadSegments = network.FindRoadSegments(roadSegmentIdsWithGeometryVersionZero.Select(x => new RoadSegmentId(x)));

        var recordNumber = RecordNumber.Initial;

        foreach (var roadSegment in roadSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            translatedChanges = translatedChanges.AppendChange(new ModifyRoadSegment(
                recordNumber,
                roadSegment.Id,
                roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? new RoadNodeId(0) : roadSegment.Start,
                roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? new RoadNodeId(0) : roadSegment.End,
                roadSegment.AttributeHash.OrganizationId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.AttributeHash.Morphology,
                roadSegment.AttributeHash.Status,
                roadSegment.AttributeHash.Category,
                roadSegment.AttributeHash.AccessRestriction,
                roadSegment.AttributeHash.LeftStreetNameId,
                roadSegment.AttributeHash.RightStreetNameId
            ).WithGeometry(roadSegment.Geometry));

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

        await _roadNetworkCommandQueue.Write(new Command(changeRoadNetwork), cancellationToken);

        return new CorrectRoadSegmentVersionsResponse(roadSegments.Count);
    }

    private async Task<List<int>> GetRoadSegmentIdsWithInvalidVersions(CancellationToken cancellationToken)
    {
        await using var context = _editorContextFactory();

        var roadSegmentIds = new List<int>();
        const int pageSize = 5000;
        var pageIndex = 0;

        while (await FillInvalidRoadSegmentIds(context, pageIndex++, pageSize, roadSegmentIds, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return roadSegmentIds;
    }

    private async Task<bool> FillInvalidRoadSegmentIds(EditorContext context, int pageIndex, int pageSize, List<int> roadSegmentIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);
        var roadSegments = await context.RoadSegments
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        sw.Restart();

        _logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);

        roadSegmentIds.AddRange(roadSegments
            .Select(x => new RoadSegmentDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .Where(x => x.WS_GIDN.Value.EndsWith("_0") || x.WS_UIDN.Value.EndsWith("_0"))
            .Select(x => x.WS_OIDN.Value));
        _logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        return roadSegments.Any();
    }
}
