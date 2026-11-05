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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Infrastructure;
using Swashbuckle.AspNetCore.Annotations;
using Version = Infrastructure.Version;

public partial class ExtractenController
{
    /// <summary>
    ///     Vraagt een extract in het hernieuwde datamodel aan op basis van een contour.
    /// </summary>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ExtractDownloadaanvraagPerContourV2))]
    [MapToApiVersion(Version.V2)]
    [HttpPost("downloadaanvragen/percontour", Name = nameof(ExtractDownloadaanvraagPerContourV2))]
    public async Task<IActionResult> ExtractDownloadaanvraagPerContourV2(
        [FromBody] ExtractDownloadaanvraagPerContourBody body,
        [FromServices] IValidator<ExtractDownloadaanvraagPerContourBody> validator,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await validator.ValidateAndThrowAsync(body, cancellationToken);

            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(body.ExterneId ?? Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());
            var contour = new WKTReader().Read(body.Contour).ToMultiPolygon().EnsureLambert08();

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                ExtractRequestId = extractRequestId,
                DownloadId = downloadId,
                Contour = contour.ToExtractGeometry(),
                Description = body.Beschrijving,
                IsInformative = body.Informatief,
                ExternalRequestId = body.ExterneId,
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
