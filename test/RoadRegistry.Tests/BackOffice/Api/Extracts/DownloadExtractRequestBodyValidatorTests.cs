namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.Extensions.Logging.Abstractions;
    using NetTopologySuite.IO;
    using Xunit;

    public class DownloadExtractRequestBodyValidatorTests
    {
        private readonly DownloadExtractRequestBodyValidator _validator;

        public DownloadExtractRequestBodyValidatorTests()
        {
            _validator = new DownloadExtractRequestBodyValidator(
                    new WKTReader(),
                    new NullLogger<DownloadExtractRequestBodyValidator>());
        }

        [Fact]
        public async Task Validate_will_not_allow_invalid_geometry()
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequestBody
            {
                RequestId = "request id",
                Contour = "invalid"
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_will_not_allow_empty_geometry(string givenContour)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequestBody
            {
                RequestId = "request id",
                Contour = givenContour
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Validate_will_allow_valid_geometry()
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequestBody
            {
                RequestId = "request id",
                Contour = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))"
            });

            await act.Should().NotThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_will_not_allow_empty_request_id(string givenRequestId)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequestBody
            {
                RequestId = givenRequestId,
                Contour = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))"
            });

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
