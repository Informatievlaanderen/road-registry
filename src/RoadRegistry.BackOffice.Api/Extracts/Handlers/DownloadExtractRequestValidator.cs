namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using FluentValidation;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed class DownloadExtractRequestValidator : AbstractValidator<DownloadExtractRequest>
{
    private readonly ILogger<DownloadExtractRequestValidator> _logger;
    private readonly WKTReader _reader;

    public DownloadExtractRequestValidator(WKTReader reader, ILogger<DownloadExtractRequestValidator> logger)
    {
        RuleFor(c => c.RequestId)
            .NotEmpty()
            .WithMessage("'RequestId' must not be empty, null or missing");
        RuleFor(c => c.Contour)
            .NotEmpty()
            .WithMessage("'Contour' must not be empty, null or missing")
            .Must(BeMultiPolygonGeometryAsWellKnownText)
            .WithMessage("'Contour' must be a valid multipolygon or polygon represented as well-known text")
            .When(c => !string.IsNullOrEmpty(c.Contour), ApplyConditionTo.CurrentValidator);

        _reader = reader;
        _logger = logger;
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
