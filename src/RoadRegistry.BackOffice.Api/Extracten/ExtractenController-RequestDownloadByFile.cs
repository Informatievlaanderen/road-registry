namespace RoadRegistry.BackOffice.Api.Extracten;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractenController
{
    /// <summary>
    ///     Requests the download by file.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="fileTranslator"></param>
    /// <param name="validator"></param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>ActionResult.</returns>
    [ProducesResponseType(typeof(LocationResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerOperation(OperationId = nameof(ExtractDownloadaanvraagPerBestand))]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [HttpPost("downloadaanvragen/perbestand", Name = nameof(ExtractDownloadaanvraagPerBestand))]
    public async Task<IActionResult> ExtractDownloadaanvraagPerBestand(
        ExtractDownloadaanvraagPerBestandBody body,
        [FromServices] IDownloadExtractByFileRequestItemTranslator fileTranslator,
        [FromServices] IValidator<ExtractDownloadaanvraagPerBestand> validator,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new ExtractDownloadaanvraagPerBestand(BuildRequestItem(".shp"), BuildRequestItem(".prj"), body.Beschrijving, body.Informatief);
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var contour = fileTranslator.Translate(request.ShpFile);
            var extractRequestId = ExtractRequestId.FromExternalRequestId(new ExternalExtractRequestId(Guid.NewGuid().ToString("N")));
            var downloadId = new DownloadId(Guid.NewGuid());

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Request = new RequestExtractRequest(extractRequestId, downloadId, contour.AsText(), request.Beschrijving, request.Informatief, null)
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
                       ?? throw new DutchValidationException([new ValidationFailure(nameof(body.Bestanden), $"Een bestand met de extensie '{extension}' ontbreekt.")]);
            var fileStream = new MemoryStream();
            file.CopyTo(fileStream);
            fileStream.Position = 0;
            return new ExtractDownloadaanvraagPerBestandItem(file.FileName, fileStream, ContentType.Parse(file.ContentType));
        }
    }
}

public sealed record ExtractDownloadaanvraagPerBestandBody(string Beschrijving, IFormFileCollection Bestanden, bool Informatief);

public sealed record ExtractDownloadaanvraagPerBestand(ExtractDownloadaanvraagPerBestandItem ShpFile, ExtractDownloadaanvraagPerBestandItem PrjFile, string Beschrijving, bool Informatief);
public sealed record ExtractDownloadaanvraagPerBestandItem(string FileName, Stream ReadStream, ContentType ContentType);

public sealed class ExtractDownloadaanvraagPerBestandValidator : AbstractValidator<ExtractDownloadaanvraagPerBestand>
{
    private readonly Encoding _encoding;

    public ExtractDownloadaanvraagPerBestandValidator()
        : this(WellKnownEncodings.WindowsAnsi)
    {
    }

    public ExtractDownloadaanvraagPerBestandValidator(Encoding encoding)
    {
        _encoding = encoding;

        RuleFor(c => c.ShpFile)
            .SetValidator(new ExtractDownloadaanvraagPerBestandItemValidator());
        RuleFor(c => c.PrjFile)
            .SetValidator(new ExtractDownloadaanvraagPerBestandItemValidator());

        RuleFor(c => c.PrjFile)
            .Must(BeLambert1972ProjectionFormat)
            .WithMessage("Projectie formaat moet Lambert 1972 zijn");

        RuleFor(c => c.Beschrijving)
            .NotNull()
            .WithProblemCode(ProblemCode.Extract.BeschrijvingIsRequired)
            .MaximumLength(ExtractDescription.MaxLength)
            .WithProblemCode(ProblemCode.Extract.BeschrijvingTooLong);
    }

    private bool BeLambert1972ProjectionFormat(ExtractDownloadaanvraagPerBestandItem item)
    {
        using var reader = new StreamReader(item.ReadStream, _encoding);
        var projectionFormat = ProjectionFormat.Read(reader);

        return projectionFormat.IsBelgeLambert1972();
    }
}

public sealed class ExtractDownloadaanvraagPerBestandItemValidator : AbstractValidator<ExtractDownloadaanvraagPerBestandItem>
{
    public ExtractDownloadaanvraagPerBestandItemValidator()
    {
        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage($"'{nameof(ExtractDownloadaanvraagPerBestandItem.FileName)}' is verplicht.");

        RuleFor(c => c.ContentType)
            .NotEmpty().WithMessage($"'{nameof(ExtractDownloadaanvraagPerBestandItem.ContentType)}' is ongeldig.");
    }
}
