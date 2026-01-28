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
using Marten;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using SqlStreamStore;

public partial class ProjectionsController
{
    [HttpGet("rebuild/topology")]
    public async Task<IActionResult> RebuildTopology(
        [FromServices] IConfiguration configuration,
        [FromQuery] int timeoutHours = 3,
        CancellationToken cancellationToken = default)
    {
        var sp = new ServiceCollection()
            .AddSingleton(configuration)
            .AddMartenRoad(options =>
            {
                options.AddRoadNetworkTopologyProjection();
            })
            .BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();
        var projectionDaemon = await store.BuildProjectionDaemonAsync();
        await projectionDaemon.RebuildProjectionAsync<RoadNetworkTopologyProjection>(TimeSpan.FromHours(timeoutHours), cancellationToken);

        return Ok($"{nameof(RoadNetworkTopologyProjection)} rebuild completed.");
    }
}
