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
    private const string GetHeadRoute = "head";

    /// <summary>
    ///     Gets the head position.
    /// </summary>
    /// <param name="maxEntryCountValue">The maximum entry count value.</param>
    /// <param name="context">The context.</param>
    /// <returns>IActionResult.</returns>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") }
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    ///     new[] { new
    ///     ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") }
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     is missing.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     requires exactly 1 value.
    /// </exception>
    /// <exception cref="Microsoft.IdentityModel.Tokens.ValidationFailure">
    ///     MaxEntryCount - MaxEntryCount query string parameter
    ///     value must be an integer.
    /// </exception>
    [HttpGet(GetHeadRoute, Name = nameof(GetHead))]
    [SwaggerOperation(OperationId = nameof(GetHead), Description = "")]
    public async Task<IActionResult> GetHead(
        [FromQuery(Name = "MaxEntryCount")] string[] maxEntryCountValue,
        [FromServices] EditorContext context)
    {
        if (maxEntryCountValue.Length == 0)
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter is missing.") });
        }

        if (maxEntryCountValue.Length != 1)
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter requires exactly 1 value.") });
        }

        if (!int.TryParse(maxEntryCountValue[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var maxEntryCount))
        {
            throw new ValidationException(new[] { new ValidationFailure("MaxEntryCount", "MaxEntryCount query string parameter value must be an integer.") });
        }

        var entries = new List<ChangeFeedEntry>();
        await context
            .RoadNetworkChanges
            .Select(change => new
            {
                change.Id,
                change.Title,
                change.Type,
                change.When
            })
            .Take(maxEntryCount)
            .OrderByDescending(_ => _.Id)
            .ForEachAsync(change =>
            {
                var when = InstantPattern.ExtendedIso.Parse(change.When).GetValueOrThrow();
                var localWhen = when.InZone(_localTimeZone).LocalDateTime;
                var item = new ChangeFeedEntry(change.Id, change.Title, change.Type, localWhen.Day.ToString("00"), _localMonthPattern.Format(localWhen.Date), _localTimeOfDayPattern.Format(localWhen.TimeOfDay));
                entries.Add(item);
            }, HttpContext.RequestAborted);

        return new JsonResult(new ChangeFeedResponse(entries.ToArray())) { StatusCode = StatusCodes.Status200OK };
    }
}