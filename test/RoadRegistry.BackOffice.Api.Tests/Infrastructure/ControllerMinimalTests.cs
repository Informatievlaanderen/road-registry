namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using Api.Infrastructure.Controllers;
using Containers;
using Editor.Schema.RoadNetworkChanges;
using Hosts.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public abstract class ControllerMinimalTests<TController> where TController : ControllerBase
{
    protected ControllerMinimalTests(IMediator mediator)
    {
        if (typeof(TController).IsAssignableTo(typeof(BackofficeApiController)))
        {
            Controller = (TController)Activator.CreateInstance(typeof(TController), new TicketingOptions(), mediator);
        }
        else
        {
            Controller = (TController)Activator.CreateInstance(typeof(TController), mediator);
        }

        Controller!.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        Mediator = mediator;
    }

    protected TController Controller { get; init; }
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