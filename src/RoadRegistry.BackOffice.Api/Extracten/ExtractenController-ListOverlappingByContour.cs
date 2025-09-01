namespace RoadRegistry.BackOffice.Api.Extracten;

using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
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
    [HttpPost("overlapping/percontour", Name = nameof(GetOverlappingPerContour))]
    public async Task<IActionResult> GetOverlappingPerContour(
        [FromBody] GetOverlappingPerContourBody body,
        [FromServices] GetOverlappingPerContourBodyValidator validator,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(body, cancellationToken);

        var response = await _mediator.Send(new GetOverlappingExtractsRequest
        {
            Contour = new WKTReader().Read(body.Contour)
        }, cancellationToken);

        return Ok(new ListOverlappingResponse(response.DownloadIds));
    }

    public class GetOverlappingPerContourBody
    {
        public string? Contour { get; set; }
    }

    public class GetOverlappingPerContourBodyValidator : AbstractValidator<GetOverlappingPerContourBody>
    {
        public GetOverlappingPerContourBodyValidator()
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
