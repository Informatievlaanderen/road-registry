namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System;
using System.Threading.Tasks;
using Containers;
using Editor.Schema.RoadNetworkChanges;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public abstract class ControllerMinimalTests<TController> where TController : ControllerBase
{
    protected ControllerMinimalTests(TController controller, IMediator mediator)
    {
        Controller = controller;
        Controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        Mediator = mediator;
    }

    protected TController Controller { get; }
    protected IMediator Mediator { get; }

    protected async Task<SqlConnectionStringBuilder> ApplyChangeCollectionIntoContext(SqlServer fixture, Func<ArchiveId, RoadNetworkChange[]> changeCallback)
    {
        var database = await fixture.CreateDatabaseAsync();
        var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));
        var changeCollection = changeCallback(archiveId);

        await using var context = await fixture.CreateEmptyEditorContextAsync(database);

        foreach (var @event in changeCollection)
        {
            context.RoadNetworkChanges.Add(@event);
        }

        await context.SaveChangesAsync();

        return database;
    }
}
