namespace RoadRegistry.BackOffice.Api.Changes;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Editor.Schema;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using Swashbuckle.AspNetCore.Annotations;

public partial class ChangeFeedController
{
    private const string GetPreviousRoute = "previous";

    /// <summary>
    ///     Gets the previous activity.
    /// </summary>
    /// <param name="beforeEntryValue">The before entry value.</param>
    /// <param name="maxEntryCountValue">The maximum entry count value.</param>
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
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("BeforeEntry", "BeforeEntry query string parameter requires exactly 1 value.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("BeforeEntry", "BeforeEntry query string parameter value must be an integer.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") }
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     BeforeEntry - BeforeEntry query string parameter is
    ///     missing.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     is missing.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     BeforeEntry - BeforeEntry query string parameter
    ///     requires exactly 1 value.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     requires exactly 1 value.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     BeforeEntry - BeforeEntry query string parameter
    ///     value must be an integer.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     value must be an integer.
    /// </exception>
    [HttpGet(GetPreviousRoute, Name = nameof(GetPrevious))]
    [SwaggerOperation(OperationId = nameof(GetPrevious), Description = "")]
    public async Task<IActionResult> GetPrevious(
        [FromQuery(Name = "BeforeEntry")] string[] beforeEntryValue,
        [FromQuery(Name = "MaxEntryCount")] string[] maxEntryCountValue,
        [FromServices] EditorContext context)
    {
        if (beforeEntryValue.Length == 0)
        {
            throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter is missing.") });
        }

        if (maxEntryCountValue.Length == 0)
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });
        }

        if (beforeEntryValue.Length != 1)
        {
            throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter requires exactly 1 value.") });
        }

        if (maxEntryCountValue.Length != 1)
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") });
        }

        if (!long.TryParse(beforeEntryValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var beforeEntry))
        {
            throw new ValidationException(new[] { new ValidationFailure("BeforeEntry", "BeforeEntry query string parameter value must be an integer.") });
        }

        if (!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount))
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") });
        }

        var entries = new List<ChangeFeedEntry>();
        await context
            .RoadNetworkChanges
            .Where(change => change.Id < beforeEntry)
            .Take(maxEntryCount)
            .OrderByDescending(_ => _.Id)
            .Select(change => new
            {
                change.Id,
                change.Title,
                change.Type,
                change.When
            })
            .ForEachAsync(change =>
            {
                var when = InstantPattern.ExtendedIso.Parse(change.When).GetValueOrThrow();
                var localWhen = when.InZone(_localTimeZone).LocalDateTime;
                var item = new ChangeFeedEntry(change.Id, change.Title, change.Type, localWhen.Day.ToString("00"), _localMonthPattern.Format(localWhen.Date), _localTimeOfDayPattern.Format(localWhen.TimeOfDay));
                entries.Add(item);
            }, HttpContext.RequestAborted);

        return new JsonResult(new ChangeFeedResponse(entries.OrderByDescending(entry => entry.Id).ToArray())) { StatusCode = StatusCodes.Status200OK };
    }
}