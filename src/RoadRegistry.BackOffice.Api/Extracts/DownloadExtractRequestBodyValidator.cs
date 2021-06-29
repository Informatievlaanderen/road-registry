namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using FluentValidation;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    public class DownloadExtractRequestBodyValidator : AbstractValidator<DownloadExtractRequestBody>
    {
        private readonly WKTReader _reader;

        public DownloadExtractRequestBodyValidator(WKTReader reader) //TODO: Add logger to be able to log why parsing the WKT failed
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));

            RuleFor(c => c.RequestId)
                .NotEmpty().WithErrorCode("RequestIdMissing");
            RuleFor(c => c.Contour)
                .NotEmpty().WithErrorCode("ContourMissing")
                .Must(BeMultiPolygonGeometryAsWellKnownText).WithErrorCode("ContourMismatch")
                .When(c => !string.IsNullOrEmpty(c.Contour), ApplyConditionTo.CurrentValidator);
        }

        private bool BeMultiPolygonGeometryAsWellKnownText(string text)
        {
            try
            {
                var geometry = _reader.Read(text);
                if (geometry is MultiPolygon multiPolygon)
                {
                    return multiPolygon.IsValid;
                }

                return false;
            }
            catch (ParseException)
            {
                return false;
            }
        }
    }
}
