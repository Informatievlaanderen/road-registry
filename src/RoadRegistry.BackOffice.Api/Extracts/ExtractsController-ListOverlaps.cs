namespace RoadRegistry.BackOffice.Api.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Gets the overlapping transaction zones.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    [HttpPost("overlapping/list", Name = nameof(ListOverlaps))]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = nameof(ListOverlaps))]
    public async Task<IActionResult> ListOverlaps(
        [FromBody] ListOverlappingParameters parameters,
        [FromServices] ListOverlappingParametersValidator validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, cancellationToken);

        var response = await _mediator.Send(new GetOverlappingTransactionZonesRequest
        {
            NisCode = parameters?.NisCode,
            Buffer = parameters?.Buffer ?? 0,
            Contour = parameters?.Contour
        }, cancellationToken);

        return Ok(response.DownloadIds);
    }

    public class ListOverlappingParameters
    {
        public string? NisCode { get; set; }
        public int Buffer { get; set; }
        public string? Contour { get; set; }
    }

    public class ListOverlappingParametersValidator : AbstractValidator<ListOverlappingParameters>
    {
        public ListOverlappingParametersValidator()
        {
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .Must(x => x.Contour is not null || x.NisCode is not null)
                .WithMessage("Contour of NisCode is verplicht.");

            RuleFor(x => x.Buffer)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Buffer moet groter of gelijk zijn aan 0.");

            When(x => x.Contour is not null, () =>
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
                    .WithMessage("Contour is geen geldige geometrie.");
            });
        }
    }
}
