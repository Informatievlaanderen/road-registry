namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using Swashbuckle.AspNetCore.Annotations;
using Version = Infrastructure.Version;

public partial class ExtractenController
{
    private const int MaxContourUploadBytes = 10 * 1024 * 1024;

    /// <summary>
    ///     Vraagt een extract in het hernieuwde datamodel aan op basis van een shapefile.
    /// </summary>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ExtractDownloadaanvraagPerBestandV2))]
    // A shapefile contour is a handful of small files; 10 MB is what the public api in front of this already caps an
    // upload at, so anything above it never gets here by the normal route anyway.
    [RequestFormLimits(MultipartBodyLengthLimit = MaxContourUploadBytes, ValueLengthLimit = MaxContourUploadBytes)]
    [MapToApiVersion(Version.V2)]
    [HttpPost("downloadaanvragen/perbestand", Name = nameof(ExtractDownloadaanvraagPerBestandV2))]
    public async Task<IActionResult> ExtractDownloadaanvraagPerBestandV2(
        ExtractDownloadaanvraagPerBestandBody body,
        [FromServices] IValidator<ExtractDownloadaanvraagPerBestand> validator,
        [FromServices] IExtractShapefileContourReader shpFileContourReader,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ExtractDownloadaanvraagPerBestand(BuildRequestItem(".shp"), BuildRequestItem(".prj"), body.Beschrijving, body.Informatief);
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var contour = shpFileContourReader.Read(request.ShpFile.ReadStream, WellKnownGeometryFactories.Lambert72WithoutMAndZ).ToMultiPolygon().EnsureLambert08();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());

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

        ExtractDownloadaanvraagPerBestandItem BuildRequestItem(string extension)
        {
            var file = body.Bestanden?.SingleOrDefault(formFile => formFile.FileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                       ?? throw new DutchValidationException([new ValidationFailure
                       {
                           PropertyName = nameof(body.Bestanden),
                           ErrorCode = "BestandVerplicht",
                           ErrorMessage = $"Een bestand met de extensie '{extension}' is verplicht."
                       }]);
            var fileStream = new MemoryStream();
            file.CopyTo(fileStream);
            fileStream.Position = 0;
            return new ExtractDownloadaanvraagPerBestandItem(file.FileName, fileStream, ContentType.Parse(file.ContentType));
        }
    }
}
