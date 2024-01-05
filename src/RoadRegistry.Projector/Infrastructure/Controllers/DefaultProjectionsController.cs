namespace RoadRegistry.Projector.Infrastructure.Controllers;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlStreamStore;

public abstract class DefaultProjectionsController : ControllerBase
{
    protected readonly Dictionary<ProjectionDetail, Func<DbContext>> Projections;
    protected readonly IStreamStore StreamStore;

    protected DefaultProjectionsController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> projections)
    {
        StreamStore = streamStore;
        Projections = projections;
    }

    protected async Task<ProjectionStateItem?> GetProjectionStateItem(string id, Func<DbContext> ctxFactory, CancellationToken cancellationToken)
    {
        await using var ctx = ctxFactory();
        var projectionStates = ctx.Set<ProjectionStateItem>();
        var projection =
            await projectionStates
                .SingleOrDefaultAsync(item => item.Name == id, cancellationToken)
                .ConfigureAwait(false);
        return projection;
    }
}
