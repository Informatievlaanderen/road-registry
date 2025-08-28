namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Gets the overlapping transaction zones by contour.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(typeof(ListOverlappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation]
    [HttpPost("overlapping/bycontour", Name = nameof(ListOverlappingByContour))]
    public async Task<IActionResult> ListOverlappingByContour(
        [FromBody] ListOverlappingByContourBody body,
        [FromServices] ListOverlappingByContourBodyValidator validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(body, cancellationToken);

        var response = await _mediator.Send(new GetOverlappingExtractsRequest
        {
            Contour = new WKTReader().Read(body.Contour)
        }, cancellationToken);

        return Ok(new ListOverlappingResponse(response.DownloadIds));
    }

    public class ListOverlappingByContourBody
    {
        public string? Contour { get; set; }
    }

    public class ListOverlappingByContourBodyValidator : AbstractValidator<ListOverlappingByContourBody>
    {
        public ListOverlappingByContourBodyValidator()
        {
            RuleFor(x => x.Contour)
                .NotEmpty().WithMessage("'Contour' is verplicht.");

            When(x => !string.IsNullOrWhiteSpace(x.Contour), () =>
            {
                RuleFor(x => x.Contour)
                    .Must(x =>
                    {
                        try
                        {
                            return new WKTReader().Read(x).IsValid;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .WithMessage("'Contour' is geen geldige geometrie.");
            });
        }
    }
}
