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
    private const string GetNextRoute = "next";

    /// <summary>
    ///     Gets the next activity.
    /// </summary>
    /// <param name="afterEntry">The after entry value.</param>
    /// <param name="maxEntryCount">The maximum entry count value.</param>
    /// <param name="filter">The text used to filter on the Title.</param>
    /// <param name="context">The context.</param>
    /// <returns>IActionResult.</returns>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new ValidationFailure("AfterEntry",
    ///     "AfterEntry query string parameter is missing.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") }
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     AfterEntry - AfterEntry query string parameter is
    ///     missing.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     is missing.
    /// </exception>
    [HttpGet(GetNextRoute, Name = nameof(GetNext))]
    [SwaggerOperation(OperationId = nameof(GetNext), Description = "")]
    public async Task<IActionResult> GetNext(
        [FromQuery] long? afterEntry,
        [FromQuery] int? maxEntryCount,
        [FromQuery] string filter,
        [FromServices] EditorContext context)
    {
        if (afterEntry is null)
        {
            throw new ValidationException(new[] { new ValidationFailure("AfterEntry", "AfterEntry query string parameter is missing.") });
        }

        if (maxEntryCount is null)
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });
        }

        var entries = await GetChangeFeedEntries(context, q => q
            .Where(change => change.Id > afterEntry)
            .Where(change => string.IsNullOrEmpty(filter) || change.Title.Contains(filter))
            .OrderBy(change => change.Id)
            .Take(maxEntryCount.Value)
        );

        return new JsonResult(new ChangeFeedResponse(entries)) { StatusCode = StatusCodes.Status200OK };
    }
}
