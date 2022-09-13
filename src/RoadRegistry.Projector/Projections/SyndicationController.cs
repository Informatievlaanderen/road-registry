namespace RoadRegistry.Projector.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Microsoft.AspNetCore.Mvc;
    using SqlStreamStore;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Response;

    [ApiVersion("1.0")]
    [ApiRoute("syndication")]
    public class SyndicationController : DefaultProjectionsController
    {
        public SyndicationController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> listOfProjections): base(streamStore, listOfProjections)
        {
        }

        [HttpGet]
        public async Task<IActionResult> ListSyndicationProjections(CancellationToken cancellationToken)
        {
            var response = new SyndicationStatusList
            {
                Syndications = new List<SyndicationStatus>()
            };

            foreach (var p in _listOfProjections)
            {
                var detail = p.Key;

                if (!detail.IsSyndication)
                {
                    continue;
                }

                var projection = await GetProjectionStateItem(detail.Id, p.Value, cancellationToken);

                if (projection == null)
                {
                    continue;
                }

                response.Syndications.Add(new SyndicationStatus
                {
                    CurrentPosition = projection.Position,
                    Name = detail.Name,
                });
            }
            return Ok(response);
        }
    }
}
