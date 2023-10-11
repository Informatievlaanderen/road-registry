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
using AcceptedChange = Editor.Schema.RoadNetworkChanges.AcceptedChange;
using RejectedChange = Editor.Schema.RoadNetworkChanges.RejectedChange;

public partial class ChangeFeedControllerTests
{
    [Fact]
    public async Task When_downloading_next_changes_of_an_empty_registry()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        var result = await Controller.GetNext(new[] { "0" }, new[] { "5" }, context);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
        var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);
        Assert.Empty(response.Entries);
    }

    [Fact]
    public async Task When_downloading_next_changes_of_filled_registry()
    {
        var database = await ApplyChangeCollectionIntoContext(_fixture, archiveId => new RoadNetworkChange[]
        {
            new()
            {
                Id = 0,
                Title = "Het opladings archief werd ontvangen.",
                Type = nameof(RoadNetworkChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveUploadedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" }
                }),
                When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
            },
            new()
            {
                Id = 1,
                Title = "Het opladings archief werd geaccepteerd.",
                Type = nameof(RoadNetworkChangesArchiveAccepted),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesArchiveAcceptedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" },
                    Files = Array.Empty<FileProblems>()
                }),
                When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
            },
            new()
            {
                Id = 2,
                Title = "De oplading werd geaccepteerd.",
                Type = nameof(RoadNetworkChangesAccepted),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesBasedOnArchiveAcceptedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" },
                    Changes = Array.Empty<AcceptedChange>()
                }),
                When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
            },
            new()
            {
                Id = 3,
                Title = "De oplading werd geweigerd.",
                Type = nameof(RoadNetworkChangesRejected),
                Content = JsonConvert.SerializeObject(new RoadNetworkChangesBasedOnArchiveRejectedEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" },
                    Changes = Array.Empty<RejectedChange>()
                }),
                When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
            },
            new()
            {
                Id = 4,
                Title = "Geen wijzigingen in de oplading.",
                Type = nameof(NoRoadNetworkChanges),
                Content = JsonConvert.SerializeObject(new NoRoadNetworkChangesBasedOnArchiveEntry
                {
                    Archive = new ArchiveInfo { Id = archiveId, Available = true, Filename = "file.zip" }
                }),
                When = InstantPattern.ExtendedIso.Format(NodaConstants.UnixEpoch)
            }
        });

        await using var editorContext = await _fixture.CreateEditorContextAsync(database);
        var result = await Controller.GetNext(new[] { "1" }, new[] { "3" }, editorContext);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.Equal(StatusCodes.Status200OK, jsonResult.StatusCode);
        var response = Assert.IsType<ChangeFeedResponse>(jsonResult.Value);

        Assert.Equal(3, response.Entries.Length);

        var item1 = response.Entries[0];
        Assert.NotNull(item1);
        Assert.Equal(4, item1.Id);
        Assert.Equal("Geen wijzigingen in de oplading.", item1.Title);
        Assert.Equal(nameof(NoRoadNetworkChanges), item1.Type);
        Assert.Equal("01", item1.Day);
        // YR: Different versions of libicu use different casing
        Assert.Equal("jan", item1.Month.ToLowerInvariant());
        Assert.Equal("01:00", item1.TimeOfDay);

        var item2 = response.Entries[1];
        Assert.NotNull(item2);
        Assert.Equal(3, item2.Id);
        Assert.Equal("De oplading werd geweigerd.", item2.Title);
        Assert.Equal(nameof(RoadNetworkChangesRejected), item2.Type);
        Assert.Equal("01", item2.Day);
        // YR: Different versions of libicu use different casing
        Assert.Equal("jan", item2.Month.ToLowerInvariant());
        Assert.Equal("01:00", item2.TimeOfDay);

        var item3 = response.Entries[2];
        Assert.NotNull(item3);
        Assert.Equal(2, item3.Id);
        Assert.Equal("De oplading werd geaccepteerd.", item3.Title);
        Assert.Equal(nameof(RoadNetworkChangesAccepted), item3.Type);
        Assert.Equal("01", item3.Day);
        // YR: Different versions of libicu use different casing
        Assert.Equal("jan", item3.Month.ToLowerInvariant());
        Assert.Equal("01:00", item3.TimeOfDay);
    }

    [Fact]
    public async Task When_downloading_next_changes_with_a_max_entry_count_that_is_not_an_integer()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetNext(new[] { "0" }, new[] { "abc" }, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") });
        }
    }

    [Fact]
    public async Task When_downloading_next_changes_with_an_after_entry_that_is_not_an_integer()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetNext(new[] { "abc" }, new[] { "0" }, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("AfterEntry", "AfterEntry query string parameter value must be an integer.") });
        }
    }

    [Fact]
    public async Task When_downloading_next_changes_with_too_many_after_entries_specified()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetNext(new[] { "1", "2" }, new[] { "0" }, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("AfterEntry", "AfterEntry query string parameter requires exactly 1 value.") });
        }
    }

    [Fact]
    public async Task When_downloading_next_changes_with_too_many_max_entry_counts_specified()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetNext(new[] { "0" }, new[] { "5", "10" }, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") });
        }
    }

    [Fact]
    public async Task When_downloading_next_changes_without_specifying_a_max_entry_count()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetNext(new[] { "0" }, new string[] { }, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });
        }
    }

    [Fact]
    public async Task When_downloading_next_changes_without_specifying_an_after_entry()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        try
        {
            await Controller.GetNext(new string[] { }, new[] { "0" }, context);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException exception)
        {
            exception.Errors.Should().BeEquivalentTo(new List<ValidationFailure> { new("AfterEntry", "AfterEntry query string parameter is missing.") });
        }
    }
}
