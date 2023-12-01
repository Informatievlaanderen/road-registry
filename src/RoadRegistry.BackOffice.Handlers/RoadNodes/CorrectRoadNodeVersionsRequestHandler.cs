namespace RoadRegistry.BackOffice.Handlers.RoadNodes;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions.RoadNodes;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadNodes;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Editor.Projections;
using RoadRegistry.Editor.Schema;
using System.Diagnostics;
using Editor.Schema.Extensions;
using ModifyRoadNode = BackOffice.Uploads.ModifyRoadNode;
using Reason = BackOffice.Reason;

public sealed class CorrectRoadNodeVersionsRequestHandler : IRequestHandler<CorrectRoadNodeVersionsRequest, CorrectRoadNodeVersionsResponse>
{
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly ILogger<CorrectRoadNodeVersionsRequestHandler> _logger;

    public CorrectRoadNodeVersionsRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadRegistryContext roadRegistryContext,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        ILogger<CorrectRoadNodeVersionsRequestHandler> logger)
    {
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _roadRegistryContext = roadRegistryContext;
        _editorContextFactory = editorContextFactory;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _logger = logger;
    }

    public async Task<CorrectRoadNodeVersionsResponse> Handle(CorrectRoadNodeVersionsRequest request, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId(Organisation.DigitaalVlaanderen.ToString()))
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Corrigeer wegknopen versies"));

        var roadNodeIdsWithGeometryVersionZero = await GetRoadNodeIdsWithInvalidVersions(cancellationToken);
        if (!roadNodeIdsWithGeometryVersionZero.Any())
        {
            return new CorrectRoadNodeVersionsResponse(0);
        }

        var network = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);

        var roadNodes = network.FindRoadNodes(roadNodeIdsWithGeometryVersionZero.Select(x => new RoadNodeId(x)));

        var recordNumber = RecordNumber.Initial;

        foreach (var roadNode in roadNodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            translatedChanges = translatedChanges.AppendChange(new ModifyRoadNode(
                recordNumber,
                roadNode.Id,
                roadNode.Type
            ).WithGeometry(roadNode.Geometry));

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
        
        return new CorrectRoadNodeVersionsResponse(roadNodes.Count);
    }

    private async Task<List<int>> GetRoadNodeIdsWithInvalidVersions(CancellationToken cancellationToken)
    {
        await using var context = _editorContextFactory();

        var roadNodeIds = new List<int>();
        const int pageSize = 5000;
        var pageIndex = 0;

        while (await FillInvalidRoadNodeIds(context, pageIndex++, pageSize, roadNodeIds, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return roadNodeIds;
    }

    private async Task<bool> FillInvalidRoadNodeIds(EditorContext context, int pageIndex, int pageSize, List<int> roadNodeIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadNodes), pageIndex, pageSize);
        var roadNodes = await context.RoadNodes
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadNodes), sw.ElapsedMilliseconds);

        sw.Restart();

        _logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadNodes), pageIndex, pageSize);

        roadNodeIds.AddRange(roadNodes
            .Select(x => new RoadNodeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .Where(x => x.WK_UIDN.Value.EndsWith("_0"))
            .Select(x => x.WK_OIDN.Value));
        _logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadNodes), sw.ElapsedMilliseconds);

        return roadNodes.Any();
    }
}
