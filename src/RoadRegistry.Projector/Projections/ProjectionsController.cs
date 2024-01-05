namespace RoadRegistry.Projector.Projections;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

[ApiVersion("1.0")]
[ApiRoute("projections")]
public class ProjectionsController : DefaultProjectionsController
{
    private readonly ILogger<ProjectionsController> _logger;

    public ProjectionsController(IStreamStore streamStore, Dictionary<ProjectionDetail, Func<DbContext>> listOfProjections, ILogger<ProjectionsController> logger)
        : base(streamStore, listOfProjections)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ListProjections(CancellationToken cancellationToken)
    {
        var response = new ProjectionsStatusList
        {
            StreamPosition = await StreamStore.ReadHeadPosition(cancellationToken),
            Projections = new List<ProjectionStatus>()
        };

        foreach (var p in Projections)
        {
            var detail = p.Key;

            if (detail.IsSyndication) continue;

            try
            {
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
                    State = state
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Error trying to load projection {ProjectionId}: {Exception}", detail.Id, ex.ToString());

                response.Projections.Add(new ProjectionStatus
                {
                    CurrentPosition = 0,
                    Description = detail.Description,
                    ErrorMessage = "Something went wrong",
                    Id = detail.Id,
                    Name = detail.Name,
                    State = "unknown"
                });
            }
        }

        return Ok(response);
    }
}
