namespace RoadRegistry.BackOffice.Api.Tests.Changes;

using System;
using Api.Changes;
using Editor.Schema;
using Editor.Schema.RoadNetworkChanges;
using Infrastructure;

public partial class ChangeFeedControllerTests : ControllerMinimalTests<ChangeFeedController>
{
    private readonly DbContextBuilder _fixture;

    public ChangeFeedControllerTests(DbContextBuilder fixture, ChangeFeedController controller) : base(controller)
    {
        _fixture = fixture.ThrowIfNull();
    }

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
