namespace RoadRegistry.Projector.Projections;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Response;
using SqlStreamStore;

[ApiVersion("1.0")]
[ApiRoute("projections")]
public class ProjectionsController : DefaultProjectionsController
{
    public ProjectionsController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> listOfProjections) : base(streamStore, listOfProjections)
    {
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

            if (detail.IsSyndication) continue;

            var projection = await GetProjectionStateItem(detail.Id, p.Value, cancellationToken);

            if (projection == null) continue;
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