namespace RoadRegistry.BackOffice.Api.Tests.Changes;

using System;
using System.Threading.Tasks;
using Api.Changes;
using Editor.Schema;
using Editor.Schema.RoadNetworkChanges;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;
using RoadRegistry.Tests.Framework.Projections;

public partial class ChangeFeedControllerTests
{
    [Fact]
    public async Task When_downloading_entry_content_of_a_non_existing_entry()
    {
        await using var context = _fixture.CreateEditorContext();

        var result = await Controller.GetContent(context, 0);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task When_downloading_entry_content_of_an_existing_entry()
    {
        var archiveId = new ArchiveId(Guid.NewGuid().ToString("N"));

        await using var editorContext = _fixture.CreateEditorContext();
        editorContext.RoadNetworkChanges.Add(new RoadNetworkChange
        {
            Id = 0,
            Title = "Het opladings archief werd ontvangen.",
            Type = nameof(RoadNetworkChangesArchiveUploaded),
            Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
            {
                Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" }
            }),
            When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
        });
        await editorContext.SaveChangesAsync();

        var result = await Controller.GetContent(editorContext, 0);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
        var response = Assert.IsType<ChangeFeedEntryContent>(jsonResult.Value);
        Assert.Equal(0, response.Id);
        Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), response.Type);
        var content = Assert.IsType<RoadNetworkChangesArchiveUploadedEntry>(response.Content);
        Assert.Equal(archiveId.ToString(), content.Archive.Id);
        Assert.True(content.Archive.Available);
        Assert.Equal("file.zip", content.Archive.Filename);
    }
}
