namespace RoadRegistry.BackOffice.Api.Extracts;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Gets the overlapping transaction zones by nis-code.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken"></param>
    [HttpPost("overlapping/byniscode", Name = nameof(ListOverlappingByNisCode))]
    [SwaggerOperation(OperationId = nameof(ListOverlappingByNisCode))]
    public async Task<IActionResult> ListOverlappingByNisCode(
        [FromBody] ListOverlappingByNisCodeParameters parameters,
        [FromServices] ListOverlappingByNisCodeParametersValidator validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(parameters, cancellationToken);

        var response = await _mediator.Send(new GetOverlappingTransactionZonesByNisCodeRequest
        {
            NisCode = parameters.NisCode!,
            Buffer = parameters.Buffer
        }, cancellationToken);

        return Ok(new ListOverlappingByNisCodeResponse(response.DownloadIds));
    }

    public class ListOverlappingByNisCodeParameters
    {
        public string? NisCode { get; set; }
        public int Buffer { get; set; }
    }

    public record ListOverlappingByNisCodeResponse(List<Guid> DownloadIds);

    public class ListOverlappingByNisCodeParametersValidator : AbstractValidator<ListOverlappingByNisCodeParameters>
    {
        public ListOverlappingByNisCodeParametersValidator()
        {
            RuleFor(c => c.NisCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("'NisCode' is verplicht.")
                .Must(BeNisCodeWithExpectedFormat).WithMessage("Ongeldige NisCode. Verwacht formaat: '12345'");

            RuleFor(c => c.Buffer)
                .InclusiveBetween(0, 100).WithMessage("'Buffer' moet een waarde tussen 0 en 100 zijn.");
        }

        private static bool BeNisCodeWithExpectedFormat(string nisCode)
        {
            return new Regex(@"^\d{5}$").IsMatch(nisCode);
        }
    }
}
