namespace RoadRegistry.BackOffice.Api.Extracten;

using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.Sync.MunicipalityRegistry;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
{
    /// <summary>
    ///     Gets the overlapping transaction zones by nis-code.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="validator"></param>
    /// <param name="municipalityContext"></param>
    /// <param name="cancellationToken"></param>
    [ProducesResponseType(typeof(ListOverlappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation]
    [HttpPost("overlapping/perniscode", Name = nameof(GetOverlappingPerNisCode))]
    public async Task<IActionResult> GetOverlappingPerNisCode(
        [FromBody] GetOverlappingPerNisCodeBody body,
        [FromServices] GetOverlappingPerNisCodeBodyValidator validator,
        [FromServices] MunicipalityEventConsumerContext municipalityContext,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(body, cancellationToken);

        var municipality = await municipalityContext.FindCurrentMunicipalityByNisCode(body.NisCode!, cancellationToken);
        if (municipality?.Geometry is null)
        {
            throw new ValidationException([new ValidationFailure
            {
                PropertyName = nameof(body.NisCode),
                ErrorCode = "NotFound",
                ErrorMessage = $"Er werd geen gemeente/stad gevonden voor de NIS-code '{body.NisCode}'"
            }]);
        }

        var contour = municipality.Geometry.ToMultiPolygon();

        var response = await _mediator.Send(new GetOverlappingExtractsRequest(contour), cancellationToken);

        return Ok(new ListOverlappingResponse(response.DownloadIds));
    }

    public class GetOverlappingPerNisCodeBody
    {
        public string? NisCode { get; set; }
    }

    public class GetOverlappingPerNisCodeBodyValidator : AbstractValidator<GetOverlappingPerNisCodeBody>
    {
        public GetOverlappingPerNisCodeBodyValidator()
        {
            RuleFor(c => c.NisCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("'NisCode' is verplicht.")
                .Must(BeNisCodeWithExpectedFormat).WithMessage("Ongeldige NisCode. Verwacht formaat: '12345'");
        }

        private static bool BeNisCodeWithExpectedFormat(string nisCode)
        {
            return new Regex(@"^\d{5}$").IsMatch(nisCode);
        }
    }
}
