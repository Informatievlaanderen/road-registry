namespace RoadRegistry.Projector.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;

    public class DefaultProjectionsController : ControllerBase
    {
        protected readonly IStreamStore _streamStore;
        protected readonly Dictionary<ProjectionDetail, Func<DbContext>> _listOfProjections;

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
}
