namespace RoadRegistry.BackOffice.Api.Changes;

using Editor.Schema;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

public partial class ChangeFeedController
{
    private const string GetPreviousRoute = "previous";

    /// <summary>
    ///     Gets the previous activity.
    /// </summary>
    /// <param name="beforeEntry">The before entry value.</param>
    /// <param name="maxEntryCount">The maximum entry count value.</param>
    /// <param name="filter">The text used to filter on the Title.</param>
    /// <param name="context">The context.</param>
    /// <returns>IActionResult.</returns>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("BeforeEntry", "BeforeEntry query string parameter is missing.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") }
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     BeforeEntry - BeforeEntry query string parameter is
    ///     missing.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     is missing.
    /// </exception>
    [HttpGet(GetPreviousRoute, Name = nameof(GetPrevious))]
    [SwaggerOperation(OperationId = nameof(GetPrevious), Description = "")]
    public async Task<IActionResult> GetPrevious(
        [FromQuery] long? beforeEntry,
        [FromQuery] int? maxEntryCount,
        [FromQuery] string filter,
        [FromServices] EditorContext context)
    {
        if (beforeEntry is null)
        {
            throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter is missing.") });
        }

        if (maxEntryCount is null)
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });
        }

        var entries = await GetChangeFeedEntries(context, q => q
            .Where(change => change.Id < beforeEntry)
            .Where(change => string.IsNullOrEmpty(filter) || change.Title.Contains(filter))
            .OrderByDescending(change => change.Id)
            .Take(maxEntryCount.Value)
        );

        return new JsonResult(new ChangeFeedResponse(entries)) { StatusCode = StatusCodes.Status200OK };
    }
}
