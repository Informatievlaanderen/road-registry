namespace RoadRegistry.Projector.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers;
using JasperFx;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using SqlStreamStore;

[ApiVersion("1.0")]
[ApiRoute("projections")]
public partial class ProjectionsController : DefaultProjectionsController
{
    private readonly IDocumentStore _documentStore;
    private readonly IReadOnlyList<ProjectionDetail> _martenProjections;
    private readonly MartenProjectionDaemonAccessor _daemonAccessor;
    private readonly MartenProjectionStateStore _projectionStateStore;
    private readonly ILogger _logger;

    public ProjectionsController(
        IStreamStore streamStore,
        IDocumentStore documentStore,
        Dictionary<ProjectionDetail, Func<DbContext>> listOfProjections,
        IReadOnlyList<ProjectionDetail> martenProjections,
        MartenProjectionDaemonAccessor daemonAccessor,
        MartenProjectionStateStore projectionStateStore,
        ILogger<ProjectionsController> logger)
        : base(streamStore, listOfProjections)
    {
        _documentStore = documentStore;
        _martenProjections = martenProjections;
        _daemonAccessor = daemonAccessor;
        _projectionStateStore = projectionStateStore;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ListProjections(CancellationToken cancellationToken)
    {
        var response = new ProjectionsStatusList
        {
            Projections = []
        };

        response.Projections.AddRange(await GetStreamStoreProjectionStatusses(cancellationToken));
        response.Projections.AddRange(await GetDocumentStoreProjectionStatusses(cancellationToken));

        return Ok(response);
    }

    private async Task<List<ProjectionStatus>> GetDocumentStoreProjectionStatusses(CancellationToken cancellationToken)
    {
        await using var session = _documentStore.LightweightSession();

        var progressions = await session.GetEventProgressions(cancellationToken);
        var storePosition = progressions.Where(x => x.Name == MartenConstants.HighWaterMarkName).Select(x => x.LastSequenceId).SingleOrDefault();

        var daemon = _daemonAccessor.Daemon;

        var desiredStates = await _projectionStateStore.GetDesiredStatesAsync(cancellationToken);

        return _martenProjections
            .Select(x =>
            {
                var lastSequenceId = progressions.SingleOrDefault(p => p.Name == x.Id)?.LastSequenceId;
                var isRunning = daemon is not null && daemon.StatusFor(x.Id) == AgentStatus.Running;

                return new ProjectionStatus
                {
                    StorePosition = storePosition,
                    CurrentPosition = lastSequenceId ?? 0,
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    // What the shard is actually doing, asked of the daemon - not what somebody wants it to do. A
                    // projection that fell over is paused, which is "stopped" here: it is not processing events, and
                    // reporting the desired state instead is what used to make a stuck projection look healthy.
                    State = isRunning ? ProjectionDesiredStates.Subscribed : ProjectionDesiredStates.Stopped,
                    ErrorMessage = DescribeMartenProjectionProblem(desiredStates.DesiredStateOf(x), isRunning, lastSequenceId)
                };
            })
            .ToList();
    }

    // The page renders this next to the state, and a projection that is down while it is supposed to be running is the
    // one case where "stopped" alone does not say enough: nobody asked for it and nobody was told.
    private static string DescribeMartenProjectionProblem(string desiredState, bool isRunning, long? lastSequenceId)
    {
        if (!isRunning && desiredState.ShouldBeRunning())
        {
            return "Stopped while it is supposed to be running; it is not processing events. Check the logs for the failure.";
        }

        return lastSequenceId is null ? "No progression found" : string.Empty;
    }

    private async Task<List<ProjectionStatus>> GetStreamStoreProjectionStatusses(CancellationToken cancellationToken)
    {
        var storePosition = await StreamStore.ReadHeadPosition(cancellationToken);

        var statuses = new List<ProjectionStatus>();

        foreach (var p in Projections)
        {
            var detail = p.Key;

            try
            {
                var projection = await GetProjectionStateItem(detail.Id, p.Value, cancellationToken);
                if (projection == null) continue;

                var state = projection.DesiredState ?? detail.FallbackDesiredState;
                statuses.Add(new ProjectionStatus
                {
                    StorePosition = storePosition,
                    CurrentPosition = projection.Position,
                    Description = detail.Description,
                    ErrorMessage = projection.ErrorMessage ?? string.Empty,
                    Id = detail.Id,
                    Name = detail.Name,
                    State = state
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error trying to load projection {ProjectionId}: {Exception}", detail.Id, ex.ToString());

                statuses.Add(new ProjectionStatus
                {
                    StorePosition = storePosition,
                    CurrentPosition = 0,
                    Description = detail.Description,
                    ErrorMessage = "Something went wrong",
                    Id = detail.Id,
                    Name = detail.Name,
                    State = "unknown"
                });
            }
        }

        return statuses;
    }
}
