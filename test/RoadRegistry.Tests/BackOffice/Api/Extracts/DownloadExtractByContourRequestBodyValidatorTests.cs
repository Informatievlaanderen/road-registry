namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.Extensions.Logging.Abstractions;
    using NetTopologySuite.IO;
    using Xunit;

    public class DownloadExtractByContourRequestBodyValidatorTests
    {
        private const string ValidContour = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))";
        private const string ValidDescription = "description";
        private const int ValidBuffer = 50;

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
                Contour = "invalid",
                Buffer = ValidBuffer,
                Description = ValidDescription
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
                Contour = givenContour,
                Buffer = ValidBuffer,
                Description = ValidDescription
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Validate_will_allow_valid_geometry()
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = ValidContour,
                Buffer = ValidBuffer,
                Description = ValidDescription
            });

            await act.Should().NotThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public async Task Validate_will_not_allow_invalid_buffer(int givenBuffer)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = ValidContour,
                Buffer = givenBuffer,
                Description = ValidDescription
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        [InlineData(100)]
        public async Task Validate_will_allow_valid_buffer(int givenBuffer)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = ValidContour,
                Buffer = givenBuffer,
                Description = ValidDescription
            });

            await act.Should().NotThrowAsync<ValidationException>();
        }

        [Theory]
        [MemberData(nameof(ValidDescriptionCases))]
        public async Task Validate_will_allow_valid_description(string givenDescription)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = ValidContour,
                Buffer = ValidBuffer,
                Description = givenDescription
            });

            await act.Should().NotThrowAsync<ValidationException>();
        }

        public static IEnumerable<object[]> ValidDescriptionCases()
        {
            yield return new object[] { string.Empty };
            yield return new object[] { "description" };
            yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray())};
        }

        [Theory]
        [MemberData(nameof(InvalidDescriptionCases))]
        public async Task Validate_will_not_allow_invalid_description(string givenDescription)
        {
            Func<Task> act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByContourRequestBody
            {
                Contour = ValidContour,
                Buffer = ValidBuffer,
                Description = givenDescription
            });

            await act.Should().ThrowAsync<ValidationException>();
        }

        public static IEnumerable<object[]> InvalidDescriptionCases()
        {
            yield return new object[] {(string) null};
            yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray())};
        }
    }
}
