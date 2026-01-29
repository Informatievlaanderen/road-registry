namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FeatureToggles;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using ValueObjects.ProblemCodes;

public partial class ExtractenController
{
    /// <summary>
    ///     Requests the download by contour.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="validator"></param>
    /// <param name="useDomainV2FeatureToggle"></param>
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
        [FromServices] UseDomainV2FeatureToggle useDomainV2FeatureToggle,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await validator.ValidateAndThrowAsync(body, cancellationToken);

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(body.ExterneId ?? Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());
            var contour = new WKTReader().Read(body.Contour).ToMultiPolygon();

            if (useDomainV2FeatureToggle.FeatureEnabled)
            {
                contour = contour.EnsureLambert08();
            }

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                ExtractRequestId = extractRequestId,
                DownloadId = downloadId,
                Contour = contour.ToExtractGeometry(),
                Description = body.Beschrijving,
                IsInformative = body.Informatief,
                ExternalRequestId = body.ExterneId,
                ZipArchiveWriterVersion = useDomainV2FeatureToggle.FeatureEnabled
                    ? WellKnownZipArchiveWriterVersions.DomainV2
                    : WellKnownZipArchiveWriterVersions.DomainV1_2
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
                .Must(ExternalExtractRequestId.AcceptsValue)
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
                return geometry.IsValid
                       && (geometry is Polygon || geometry is MultiPolygon)
                       && geometry.Area <= (SquareKmMaximum * 1000 * 1000);
            }
            catch
            {
                return false;
            }
        }
    }
}
