namespace RoadRegistry.Projector.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Mvc;
    using SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Response;

    [ApiVersion("1.0")]
    [ApiRoute("projections")]
    public class DefaultProjectionsController : ControllerBase
    {
        private readonly IStreamStore _streamStore;
        private readonly Dictionary<ProjectionDetail, Func<DbContext>> _listOfProjections;

        public DefaultProjectionsController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> listOfProjections)
        {
            _streamStore = streamStore;
            _listOfProjections = listOfProjections;
        }

        [HttpGet]
        public async Task<IActionResult> ListProjections(CancellationToken cancellationToken)
        {
            var response = new ProjectionsStatusList
            {
                StreamPosition = await _streamStore.ReadHeadPosition(cancellationToken),
                Projections = new List<ProjectionStatus>()
            };

            foreach (var p in _listOfProjections)
            {
                var detail = p.Key;
                await using var ctx = p.Value();
                var projectionStates = ctx.Set<ProjectionStateItem>();
                var projection =
                     await projectionStates
                         .SingleOrDefaultAsync(item => item.Name == detail.Id, cancellationToken)
                         .ConfigureAwait(false);

                if (projection == null)
                {
                    continue;
                }
                var state = projection.DesiredState ?? detail.FallbackDesiredState;
                response.Projections.Add(new ProjectionStatus
                {
                    CurrentPosition = projection.Position,
                    Description = detail.Description,
                    ErrorMessage = projection.ErrorMessage ?? string.Empty,
                    Id = detail.Id,
                    Name = detail.Name,
                    State = state ?? "unknown"
                });
            }
            return Ok(response);
        }



    }
}
