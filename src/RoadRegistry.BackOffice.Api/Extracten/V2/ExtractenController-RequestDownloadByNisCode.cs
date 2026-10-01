namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using Sync.MunicipalityRegistry;
using Version = Infrastructure.Version;

public partial class ExtractenController
{
    /// <summary>
    ///     Vraagt een extract in het hernieuwde datamodel aan op basis van een gemeente.
    /// </summary>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ExtractDownloadaanvraagPerNisCodeV2))]
    [MapToApiVersion(Version.V2)]
    [HttpPost("downloadaanvragen/perniscode", Name = nameof(ExtractDownloadaanvraagPerNisCodeV2))]
    public async Task<IActionResult> ExtractDownloadaanvraagPerNisCodeV2(
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
            var contour = municipality.Geometry.ToMultiPolygon().EnsureLambert08();

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                ExtractRequestId = extractRequestId,
                DownloadId = downloadId,
                Contour = contour.ToExtractGeometry(),
                Description = body.Beschrijving,
                IsInformative = body.Informatief,
                ExternalRequestId = null,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV2_Bijhouding
            }, cancellationToken);

            return Accepted(result, new ExtractDownloadaanvraagResponse(downloadId));
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
