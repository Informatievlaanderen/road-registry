namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using Editor.Schema;
using Editor.Schema.RoadNetworkChanges;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    protected async Task<EditorContext> ApplyChangeCollectionIntoContext(DbContextBuilder fixture, Func<ArchiveId, RoadNetworkChange[]> changeCallback)
    {
        var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));
        var changeCollection = changeCallback(archiveId);

        var context = fixture.CreateEditorContext();

        foreach (var @event in changeCollection)
        {
            context.RoadNetworkChanges.Add(@event);
        }

        await context.SaveChangesAsync();

        return context;
    }
}
