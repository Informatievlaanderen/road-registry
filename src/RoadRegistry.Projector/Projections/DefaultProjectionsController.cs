namespace RoadRegistry.Projector.Projections;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlStreamStore;

public class DefaultProjectionsController : ControllerBase
{
    protected readonly Dictionary<ProjectionDetail, Func<DbContext>> _listOfProjections;
    protected readonly IStreamStore _streamStore;

    public DefaultProjectionsController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> listOfProjections)
    {
        _streamStore = streamStore;
        _listOfProjections = listOfProjections;
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