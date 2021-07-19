namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using FluentValidation;
    using Microsoft.Extensions.Logging;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public class DownloadExtractRequestBodyValidator : AbstractValidator<DownloadExtractRequestBody>
    {
        private readonly WKTReader _reader;
        private readonly ILogger<DownloadExtractRequestBodyValidator> _logger;

        public DownloadExtractRequestBodyValidator(WKTReader reader, ILogger<DownloadExtractRequestBodyValidator> logger)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RuleFor(c => c.RequestId)
                .NotEmpty().WithMessage("'RequestId' must not be empty, null or missing");
            RuleFor(c => c.Contour)
                .NotEmpty().WithMessage("'Contour' must not be empty, null or missing")
                .Must(BeMultiPolygonGeometryAsWellKnownText)
                .WithMessage("'Contour' must be a valid multipolygon or polygon represented as well-known text")
                .When(c => !string.IsNullOrEmpty(c.Contour), ApplyConditionTo.CurrentValidator);
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
}
