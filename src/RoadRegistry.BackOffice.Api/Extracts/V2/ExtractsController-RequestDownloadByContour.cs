namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using BackOffice.Handlers.Information;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Core.ProblemCodes;
using Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
{
    /// <summary>
    ///     Requests the download by contour.
    /// </summary>
    /// <param name="validator"></param>
    /// <param name="body"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(RequestDownloadByContour))]
    [HttpPost("downloadrequests/bycontour", Name = nameof(RequestDownloadByContour))]
    public async Task<IActionResult> RequestDownloadByContour(
        [FromBody] RequestDownloadByContourBody body,
        [FromServices] IValidator<RequestDownloadByContourBody> validator,
        CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(body, cancellationToken);

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(body.ExternalRequestId ?? Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Request = new RequestExtractRequest(extractRequestId, downloadId, body.Contour, body.Description, body.IsInformative, body.ExternalRequestId)
            }, cancellationToken);

            return Accepted(result, new RequestExtractResponse(downloadId));
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

public record RequestDownloadByContourBody(string Contour, string Description, bool IsInformative, string? ExternalRequestId);

public class RequestDownloadByContourBodyValidator : AbstractValidator<RequestDownloadByContourBody>
{
    public RequestDownloadByContourBodyValidator()
    {
        RuleFor(x => x.Contour)
            .Must(x => new ExtractContourValidator().IsValid(x))
            .WithProblemCode(ProblemCode.Extract.GeometryInvalid);

        RuleFor(c => c.Description)
            .NotNull()
            .WithProblemCode(ProblemCode.Extract.DescriptionIsRequired)
            .MaximumLength(ExtractDescription.MaxLength)
            .WithProblemCode(ProblemCode.Extract.DescriptionTooLong);

        When(x => !string.IsNullOrEmpty(x.ExternalRequestId), () =>
        {
            RuleFor(x => x.ExternalRequestId)
                .Must(ExtractRequestId.Accepts)
                .WithProblemCode(ProblemCode.Extract.ExternalRequestIdInvalid);
        });
    }
}

public class ExtractContourValidator
{
    private const int SquareKmMaximum = 100;

    public bool IsValid(string contour)
    {
        try
        {
            var reader = new WKTReader();
            var geometry = reader.Read(contour);
            return geometry.IsValid && geometry.Area <= (SquareKmMaximum * 1000 * 1000);
        }
        catch
        {
            return false;
        }
    }
}
