namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.Sync.MunicipalityRegistry;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
{
    /// <summary>
    ///     Requests the download by nis code.
    /// </summary>
    /// <param name="body">The body.</param>
    /// <param name="validator"></param>
    /// <param name="municipalityContext"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ExtractDownloadaanvraagPerNisCode))]
    [HttpPost("downloadaanvragen/perniscode", Name = nameof(ExtractDownloadaanvraagPerNisCode))]
    public async Task<IActionResult> ExtractDownloadaanvraagPerNisCode(
        [FromBody] ExtractDownloadaanvraagPerNisCodeBody body,
        [FromServices] IValidator<ExtractDownloadaanvraagPerNisCodeBody> validator,
        [FromServices] MunicipalityEventConsumerContext municipalityContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await validator.ValidateAndThrowAsync(body, cancellationToken);

            var municipality = await municipalityContext.FindCurrentMunicipalityByNisCode(body.NisCode, cancellationToken);
            if (municipality?.Geometry is null)
            {
                throw new ValidationException([new ValidationFailure
                {
                    PropertyName = nameof(body.NisCode),
                    ErrorCode = "NotFound",
                    ErrorMessage = $"Er werd geen gemeente/stad gevonden voor de NIS-code '{body.NisCode}'"
                }]);
            }

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());
            var contour = municipality.Geometry.ToMultiPolygon();

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Request = new RequestExtractRequest(extractRequestId, downloadId, contour.AsText(), body.Beschrijving, body.Informatief, null)
            }, cancellationToken);

            return Accepted(result, new ExtractDownloadaanvraagResponse(downloadId));
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

public record ExtractDownloadaanvraagPerNisCodeBody(string NisCode, string Beschrijving, bool Informatief);

public class ExtractDownloadaanvraagPerNisCodeBodyValidator : AbstractValidator<ExtractDownloadaanvraagPerNisCodeBody>
{
    private readonly MunicipalityEventConsumerContext _municipalityContext;

    public ExtractDownloadaanvraagPerNisCodeBodyValidator(MunicipalityEventConsumerContext municipalityContext)
    {
        _municipalityContext = municipalityContext;

        RuleFor(c => c.NisCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithProblemCode(ProblemCode.Extract.NisCodeIsRequired)
            .Must(BeNisCodeWithExpectedFormat).WithProblemCode(ProblemCode.Extract.NisCodeInvalid)
            .Must(BeKnownNisCode).WithErrorCode("NotFound").WithMessage(body => $"Er werd geen gemeente/stad gevonden voor de NIS-code '{body.NisCode}'");

        RuleFor(c => c.Beschrijving)
            .NotEmpty().WithProblemCode(ProblemCode.Extract.BeschrijvingIsRequired)
            .MaximumLength(ExtractDescription.MaxLength).WithProblemCode(ProblemCode.Extract.BeschrijvingTooLong);
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
