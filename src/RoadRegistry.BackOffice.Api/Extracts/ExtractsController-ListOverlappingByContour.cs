namespace RoadRegistry.BackOffice.Api.Extracts;

using System;
using System.Collections.Generic;
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
    ///     Gets the overlapping transaction zones by contour.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    [HttpPost("overlapping/bycontour", Name = nameof(ListOverlappingByContour))]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = nameof(ListOverlappingByContour))]
    public async Task<IActionResult> ListOverlappingByContour(
        [FromBody] ListOverlappingByContourParameters parameters,
        [FromServices] ListOverlappingByContourParametersValidator validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, cancellationToken);

        var response = await _mediator.Send(new GetOverlappingTransactionZonesByContourRequest
        {
            Contour = parameters.Contour!
        }, cancellationToken);

        return Ok(new ListOverlappingByContourResponse(response.DownloadIds));
    }

    public class ListOverlappingByContourParameters
    {
        public string? Contour { get; set; }
    }

    public record ListOverlappingByContourResponse(List<Guid> DownloadIds);

    public class ListOverlappingByContourParametersValidator : AbstractValidator<ListOverlappingByContourParameters>
    {
        public ListOverlappingByContourParametersValidator()
        {
            RuleFor(x => x.Contour)
                .NotEmpty().WithMessage("'Contour' is verplicht.")
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
        }
    }
}
