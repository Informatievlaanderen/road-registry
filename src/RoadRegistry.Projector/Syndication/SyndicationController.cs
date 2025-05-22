namespace RoadRegistry.Projector.Syndication;

using System;
using System.Collections.Generic;
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
    public Task<IActionResult> ListSyndicationProjections()
    {
        return Task.FromResult<IActionResult>(Ok(Array.Empty<object>()));
    }
}
