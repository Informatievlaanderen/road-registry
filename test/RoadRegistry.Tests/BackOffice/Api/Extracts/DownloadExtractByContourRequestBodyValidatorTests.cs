namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.Extensions.Logging.Abstractions;
    using NetTopologySuite.IO;
    using Xunit;

    public class DownloadExtractByContourRequestBodyValidatorTests
    {
        private readonly DownloadExtractByContourRequestBodyValidator _validator;

        public DownloadExtractByContourRequestBodyValidatorTests()
        {
            _validator = new DownloadExtractByContourRequestBodyValidator(
                new WKTReader(),
                new NullLogger<DownloadExtractByContourRequestBodyValidator>());
        }

        [Fact]
        public async Task Validate_will_not_allow_invalid_geometry()
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = "invalid"
            });
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_will_not_allow_empty_geometry(string givenContour)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = givenContour
            });
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Validate_will_allow_valid_geometry()
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))"
            });

            await act.Should().NotThrowAsync<ValidationException>();
        }
    }
}
