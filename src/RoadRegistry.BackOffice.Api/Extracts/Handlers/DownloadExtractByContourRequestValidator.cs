namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public sealed class DownloadExtractByContourRequestValidator : AbstractValidator<DownloadExtractByContourRequest>, IPipelineBehavior<DownloadExtractByContourRequest, DownloadExtractByContourResponse>
{
    private readonly ILogger<DownloadExtractByContourRequestValidator> _logger;
    private readonly WKTReader _reader;

    public DownloadExtractByContourRequestValidator(WKTReader reader, ILogger<DownloadExtractByContourRequestValidator> logger)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RuleFor(c => c.Contour)
            .NotEmpty().WithMessage("'Contour' must not be empty, null or missing")
            .Must(BeMultiPolygonGeometryAsWellKnownText).WithMessage("'Contour' must be a valid multipolygon or polygon represented as well-known text")
            .When(c => !string.IsNullOrEmpty(c.Contour), ApplyConditionTo.CurrentValidator);

        RuleFor(c => c.Buffer)
            .InclusiveBetween(0, 100).WithMessage("'Buffer' must be a value between 0 and 100");

        RuleFor(c => c.Description)
            .NotNull().WithMessage("'Description' must not be null or missing")
            .MaximumLength(ExtractDescription.MaxLength).WithMessage($"'Description' must not be longer than {ExtractDescription.MaxLength} characters");
    }

    public async Task<DownloadExtractByContourResponse> Handle(DownloadExtractByContourRequest request, RequestHandlerDelegate<DownloadExtractByContourResponse> next, CancellationToken cancellationToken)
    {
        await this.ValidateAndThrowAsync(request, cancellationToken);
        var response = await next();
        return response;
    }

    private bool BeMultiPolygonGeometryAsWellKnownText(string text)
    {
        try
        {
            var geometry = _reader.Read(text);
            return geometry switch
            {
                MultiPolygon multiPolygon => multiPolygon.IsValid,
                Polygon polygon => polygon.IsValid,
                _ => false
            };
        }
        catch (ParseException exception)
        {
            _logger.LogWarning(exception, "The download extract request body validation encountered a problem while trying to parse the contour as well-known text");
            return false;
        }
    }
}
