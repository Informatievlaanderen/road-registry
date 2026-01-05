namespace RoadRegistry.BackOffice.Api.Inwinning;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Extracten;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Extensions;
using Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using Sync.MunicipalityRegistry;
using ValueObjects.ProblemCodes;

public partial class InwinningController
{
    /// <summary>
    ///     Requests the download by nis code.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="validator"></param>
    /// <param name="municipalityContext"></param>
    /// <param name="options"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(RequestInwinningExtract))]
    [HttpPost("downloadaanvraag", Name = nameof(RequestInwinningExtract))]
    public async Task<IActionResult> RequestInwinningExtract(
        [FromBody] InwinningExtractDownloadaanvraagBody body,
        [FromServices] IValidator<InwinningExtractDownloadaanvraagBody> validator,
        [FromServices] MunicipalityEventConsumerContext municipalityContext,
        [FromServices] IOptions<InwinningOrganizationNisCodesOptions> options,
        CancellationToken cancellationToken = default)
    {
        var @operator = ApiContext.HttpContextAccessor.HttpContext.GetOperator();
        if (@operator is null
            || !options.Value.TryGetValue(@operator, out var niscodes)
            || !niscodes.Contains(body.NisCode))
        {
            return Forbid();
        }

        await validator.ValidateAndThrowAsync(body, cancellationToken);

        var municipality = await municipalityContext.FindCurrentMunicipalityByNisCode(body.NisCode, cancellationToken);
        if (municipality?.Geometry is null)
        {
            throw new ValidationException([
                new ValidationFailure
                {
                    PropertyName = nameof(body.NisCode),
                    ErrorCode = "NotFound",
                    ErrorMessage = $"Er werd geen gemeente/stad gevonden voor de NIS-code '{body.NisCode}'"
                }
            ]);
        }

        try
        {
            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(body.NisCode));
            var downloadId = new DownloadId(Guid.NewGuid());
            var contour = municipality.Geometry.ToMultiPolygon();

            var result = await _mediator.Send(new RequestInwinningExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                ExtractRequestId = extractRequestId,
                DownloadId = downloadId,
                Contour = contour,
                NisCode = body.NisCode
            }, cancellationToken);

            return Accepted(result, new ExtractDownloadaanvraagResponse(downloadId));
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

public record InwinningExtractDownloadaanvraagBody(string NisCode);

public class InwinningExtractDownloadaanvraagBodyValidator : AbstractValidator<InwinningExtractDownloadaanvraagBody>
{
    private readonly MunicipalityEventConsumerContext _municipalityContext;

    public InwinningExtractDownloadaanvraagBodyValidator(MunicipalityEventConsumerContext municipalityContext)
    {
        _municipalityContext = municipalityContext;

        RuleFor(c => c.NisCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithProblemCode(ProblemCode.Extract.NisCodeIsRequired)
            .Must(BeNisCodeWithExpectedFormat).WithProblemCode(ProblemCode.Extract.NisCodeInvalid)
            .Must(BeKnownNisCode).WithErrorCode("NotFound").WithMessage(body => $"Er werd geen gemeente/stad gevonden voor de NIS-code '{body.NisCode}'");
    }

    private bool BeKnownNisCode(string nisCode)
    {
        // FluentValidation does not support async validators in this context
        return _municipalityContext.CurrentMunicipalityExistsByNisCode(nisCode, CancellationToken.None).GetAwaiter().GetResult();
    }

    private static bool BeNisCodeWithExpectedFormat(string nisCode)
    {
        return new Regex(@"^\d{5}$").IsMatch(nisCode);
    }
}
