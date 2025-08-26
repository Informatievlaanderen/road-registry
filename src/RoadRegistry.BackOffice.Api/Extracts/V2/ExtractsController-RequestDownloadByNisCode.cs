namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using Swashbuckle.AspNetCore.Annotations;
using Sync.MunicipalityRegistry;

public partial class ExtractsController
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
    [SwaggerOperation(OperationId = nameof(RequestDownloadByNisCode))]
    [HttpPost("downloadrequests/byniscode", Name = nameof(RequestDownloadByNisCode))]
    public async Task<IActionResult> RequestDownloadByNisCode(
        [FromBody] RequestDownloadByNisCodeBody body,
        [FromServices] IValidator<RequestDownloadByNisCodeBody> validator,
        [FromServices] MunicipalityEventConsumerContext municipalityContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(body, cancellationToken);

            var municipality = await municipalityContext.FindCurrentMunicipalityByNisCode(body.NisCode, cancellationToken);
            if (municipality?.Geometry is null)
            {
                return NotFound();
            }

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());
            var contour = municipality.Geometry.ToMultiPolygon();

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Request = new RequestExtractRequest(extractRequestId, downloadId, contour.AsText(), body.Description, body.IsInformative, null)
            }, cancellationToken);

            return Accepted(result, new RequestExtractResponse(downloadId));
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

public sealed record RequestExtractResponse(string DownloadId);

public record RequestDownloadByNisCodeBody(string NisCode, string Description, bool IsInformative);

public class RequestDownloadByNisCodeBodyValidator : AbstractValidator<RequestDownloadByNisCodeBody>
{
    private readonly MunicipalityEventConsumerContext _municipalityContext;

    public RequestDownloadByNisCodeBodyValidator(MunicipalityEventConsumerContext municipalityContext)
    {
        _municipalityContext = municipalityContext;

        RuleFor(c => c.NisCode)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("NisCode is verplicht.")
            .Must(BeNisCodeWithExpectedFormat).WithMessage("Ongeldige NIS-code. Verwacht formaat: '12345'")
            .Must(BeKnownNisCode).WithMessage("NisCode is niet gekend.");

        RuleFor(c => c.Description)
            .NotNull()
            .WithProblemCode(ProblemCode.Extract.DescriptionIsRequired)
            .MaximumLength(ExtractDescription.MaxLength)
            .WithProblemCode(ProblemCode.Extract.DescriptionTooLong);
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
