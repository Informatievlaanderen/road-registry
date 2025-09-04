namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
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
    [SwaggerOperation(OperationId = nameof(ExtractDownloadaanvraagPerContour))]
    [HttpPost("downloadaanvragen/percontour", Name = nameof(ExtractDownloadaanvraagPerContour))]
    public async Task<IActionResult> ExtractDownloadaanvraagPerContour(
        [FromBody] ExtractDownloadaanvraagPerContourBody body,
        [FromServices] IValidator<ExtractDownloadaanvraagPerContourBody> validator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await validator.ValidateAndThrowAsync(body, cancellationToken);

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(body.ExterneId ?? Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Request = new RequestExtractRequest(extractRequestId, downloadId, body.Contour, body.Beschrijving, body.Informatief, body.ExterneId)
            }, cancellationToken);

            return Accepted(result, new ExtractDownloadaanvraagResponse(downloadId));
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}

public record ExtractDownloadaanvraagPerContourBody(string Contour, string Beschrijving, bool Informatief, string? ExterneId);

public class ExtractDownloadaanvraagPerContourBodyValidator : AbstractValidator<ExtractDownloadaanvraagPerContourBody>
{
    public ExtractDownloadaanvraagPerContourBodyValidator()
    {
        RuleFor(x => x.Contour)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Extract.ContourIsRequired)
            .Must(x => new ExtractContourValidator().IsValid(x))
            .WithProblemCode(ProblemCode.Extract.ContourInvalid)
            ;

        RuleFor(c => c.Beschrijving)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithProblemCode(ProblemCode.Extract.BeschrijvingIsRequired)
            .MaximumLength(ExtractDescription.MaxLength)
            .WithProblemCode(ProblemCode.Extract.BeschrijvingTooLong);

        When(x => !string.IsNullOrEmpty(x.ExterneId), () =>
        {
            RuleFor(x => x.ExterneId)
                .Must(ExtractRequestId.Accepts)
                .WithProblemCode(ProblemCode.Extract.ExterneIdInvalid);
        });
    }

    private sealed class ExtractContourValidator
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
}
