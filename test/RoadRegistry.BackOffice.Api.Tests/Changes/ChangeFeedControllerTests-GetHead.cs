namespace RoadRegistry.BackOffice.Api.Tests.Changes;

using Api.Changes;
using Editor.Schema.RoadNetworkChanges;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;
using Xunit.Sdk;

public partial class ChangeFeedControllerTests
{
    [Fact]
    public async Task When_downloading_head_changes_of_an_empty_registry()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        var result = await Controller.GetHead(5, null, context);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
        var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
        Assert.Empty(response.Entries);
    }

    [Fact]
    public async Task When_downloading_head_changes_of_filled_registry()
    {
        var database = await ApplyChangeCollectionIntoContext(_fixture, archiveId => new RoadNetworkChange[]
        {
            new()
            {
                Title = "De oplading werd ontvangen.",
                Type = nameof(RoadNetworkChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" }
                }),
                When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
            }
        });

        await using var editorContext = await _fixture.CreateEditorContextAsync(database);
        var result = await Controller.GetHead(5, null, editorContext);

        var jsonResult = Assert.IsType<JsonResult>(result);

        Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);

        var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
        var item = Assert.Single(response.Entries);

        Assert.NotNull(item);
        Assert.Equal(0, item.Id);
        Assert.Equal("De oplading werd ontvangen.", item.Title);
        Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), item.Type);
        Assert.Equal("01", item.Day);

        // YR: Different versions of libicu use different casing
        Assert.Equal("jan", item.Month.ToLowerInvariant());
        Assert.Equal("01:00", item.TimeOfDay);
    }
    
    [Fact]
    public async Task When_downloading_head_changes_without_specifying_a_max_entry_count()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetHead(null, null, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });
        }
    }
}
