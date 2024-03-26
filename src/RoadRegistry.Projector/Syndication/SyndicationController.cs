namespace RoadRegistry.Projector.Syndication;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlStreamStore;

[ApiVersion("1.0")]
[ApiRoute("syndication")]
public class SyndicationController : DefaultProjectionsController
{
    public SyndicationController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> projections) : base(streamStore, projections)
    {
    }

    [HttpGet]
    public async Task<IActionResult> ListSyndicationProjections(CancellationToken cancellationToken)
    {
        var response = new List<SyndicationStatus>();

        foreach (var p in Projections)
        {
            var detail = p.Key;

            if (!detail.IsSyndication) continue;

            var projection = await GetProjectionStateItem(detail.Id, p.Value, cancellationToken);
            if (projection == null) continue;

            response.Add(new SyndicationStatus
            {
                Position = projection.Position,
                ProjectionName = detail.Name
            });
        }

        return Ok(response);
    }
}
