namespace RoadRegistry.BackOffice.Api.Extracts.V2;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts.V2;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public partial class ExtractsController
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
    [SwaggerOperation(OperationId = nameof(RequestDownloadByFile))]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    [HttpPost("downloadrequests/byfile", Name = nameof(RequestDownloadByFile))]
    public async Task<IActionResult> RequestDownloadByFile(
        RequestDownloadByFileBody body,
        [FromServices] IDownloadExtractByFileRequestItemTranslator fileTranslator,
        [FromServices] IValidator<DownloadExtractByFileRequest> validator,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new DownloadExtractByFileRequest(BuildRequestItem(".shp"), BuildRequestItem(".prj"), body.Description, body.IsInformative);
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var contour = fileTranslator.Translate(request.ShpFile);
            var extractRequestId = Guid.NewGuid().ToString("N");

            var result = await _mediator.Send(new RequestExtractSqsRequest
            {
                ProvenanceData = CreateProvenanceData(Modification.Insert),
                Request = new RequestExtractRequest(extractRequestId, contour.AsText(), request.Description, request.IsInformative)
            }, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }

        DownloadExtractByFileRequestItem BuildRequestItem(string extension)
        {
            var file = body.Files?.SingleOrDefault(formFile => formFile.FileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
                       ?? throw new DutchValidationException([new ValidationFailure(nameof(body.Files), $"Een bestand met de extensie '{extension}' ontbreekt.")]);
            var fileStream = new MemoryStream();
            file.CopyTo(fileStream);
            fileStream.Position = 0;
            return new DownloadExtractByFileRequestItem(file.FileName, fileStream, ContentType.Parse(file.ContentType));
        }
    }
}

public sealed record RequestDownloadByFileBody(string Description, IFormFileCollection Files, bool IsInformative);

public sealed record DownloadExtractByFileRequest(DownloadExtractByFileRequestItem ShpFile, DownloadExtractByFileRequestItem PrjFile, string Description, bool IsInformative);
public sealed record DownloadExtractByFileRequestItem(string FileName, Stream ReadStream, ContentType ContentType);

public sealed class DownloadExtractByFileRequestValidator : AbstractValidator<DownloadExtractByFileRequest>
{
    private readonly Encoding _encoding;

    public DownloadExtractByFileRequestValidator()
        : this(WellKnownEncodings.WindowsAnsi)
    {
    }

    public DownloadExtractByFileRequestValidator(Encoding encoding)
    {
        _encoding = encoding;

        RuleFor(c => c.ShpFile)
            .SetValidator(new DownloadExtractByFileRequestItemValidator());
        RuleFor(c => c.PrjFile)
            .SetValidator(new DownloadExtractByFileRequestItemValidator());

        RuleFor(c => c.PrjFile)
            .Must(BeLambert1972ProjectionFormat)
            .WithMessage("Projection format must be Lambert 1972");

        RuleFor(c => c.Description)
            .NotNull().WithMessage("'Description' must not be null or missing")
            .MaximumLength(ExtractDescription.MaxLength).WithMessage($"'Description' must not be longer than {ExtractDescription.MaxLength} characters");
    }

    private bool BeLambert1972ProjectionFormat(DownloadExtractByFileRequestItem item)
    {
        using var reader = new StreamReader(item.ReadStream, _encoding);
        var projectionFormat = ProjectionFormat.Read(reader);

        return projectionFormat.IsBelgeLambert1972();
    }
}

public sealed class DownloadExtractByFileRequestItemValidator : AbstractValidator<DownloadExtractByFileRequestItem>
{
    public DownloadExtractByFileRequestItemValidator()
    {
        RuleFor(c => c.FileName)
            .NotEmpty().WithMessage($"'{nameof(DownloadExtractByFileRequestItem.FileName)}' must not be empty, null or missing");

        RuleFor(c => c.ContentType)
            .NotEmpty().WithMessage($"'{nameof(DownloadExtractByFileRequestItem.ContentType)}' must be able to parse");
    }
}
