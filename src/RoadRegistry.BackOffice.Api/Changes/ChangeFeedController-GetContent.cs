namespace RoadRegistry.BackOffice.Api.Changes;

using System.Threading.Tasks;
using Editor.Schema;
using Editor.Schema.RoadNetworkChanges;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

public partial class ChangeFeedController
{
    private const string GetContentRoute = "entry/{id}/content";

    /// <summary>
    ///     Gets the content of an activity.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="id">The identifier.</param>
    [HttpGet(GetContentRoute, Name = nameof(GetContent))]
    [SwaggerOperation(OperationId = nameof(GetContent), Description = "")]
    public async Task<IActionResult> GetContent([FromServices] EditorContext context, long id)
    {
        var entry = await context
            .RoadNetworkChanges
            .SingleOrDefaultAsync(change => change.Id == id, HttpContext.RequestAborted);
        if (entry == null)
        {
            return NotFound();
        }

        var content = entry.Type switch
        {
            nameof(RoadNetworkChangesArchiveUploaded) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkChangesArchiveUploadedEntry)),
            nameof(RoadNetworkChangesArchiveAccepted) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkChangesArchiveAcceptedEntry)),
            nameof(RoadNetworkChangesArchiveRejected) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkChangesArchiveRejectedEntry)),
            nameof(RoadNetworkChangesAccepted) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkChangesBasedOnArchiveAcceptedEntry)),
            nameof(RoadNetworkChangesAccepted) + ":v2" => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkChangesBasedOnArchiveAcceptedEntry)),
            nameof(RoadNetworkChangesRejected) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkChangesBasedOnArchiveRejectedEntry)),
            nameof(NoRoadNetworkChanges) => JsonConvert.DeserializeObject(entry.Content, typeof(NoRoadNetworkChangesBasedOnArchiveEntry)),
            nameof(RoadNetworkExtractChangesArchiveUploaded) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkExtractChangesArchiveUploadedEntry)),
            nameof(RoadNetworkExtractChangesArchiveAccepted) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkExtractChangesArchiveAcceptedEntry)),
            nameof(RoadNetworkExtractChangesArchiveRejected) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkExtractChangesArchiveRejectedEntry)),
            nameof(RoadNetworkExtractDownloadBecameAvailable) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkExtractDownloadBecameAvailableEntry)),
            nameof(RoadNetworkExtractDownloadTimeoutOccurred) => JsonConvert.DeserializeObject(entry.Content, typeof(RoadNetworkExtractDownloadTimeoutOccurredEntry)),
            _ => null
        };
        return new JsonResult(new ChangeFeedEntryContent(entry.Id, entry.Type, content)) { StatusCode = StatusCodes.Status200OK };
    }
}
