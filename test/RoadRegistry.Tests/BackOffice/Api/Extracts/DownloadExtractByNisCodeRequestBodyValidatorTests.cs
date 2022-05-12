namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Editor.Schema;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.Extensions.Logging.Abstractions;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class DownloadExtractByNisCodeRequestBodyValidatorTests
    {
        private const int ValidBuffer = 50;
        private const string ValidDescription = "description";
        private readonly SqlServer _sqlServerFixture;

        public DownloadExtractByNisCodeRequestBodyValidatorTests(SqlServer sqlServerFixture)
        {
            _sqlServerFixture = sqlServerFixture;
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_will_not_allow_empty_niscodes(string givenNisCode)
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = givenNisCode,
                    Buffer = ValidBuffer,
                    Description = ValidDescription
                });
                await act.Should().ThrowAsync<ValidationException>();
            }
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("1234")]
        [InlineData("123456")]
        [InlineData("1234A")]
        public async Task Validate_will_not_allow_invalid_niscodes(string givenNisCode)
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = givenNisCode,
                    Buffer = ValidBuffer,
                    Description = ValidDescription
                });
                await act.Should().ThrowAsync<ValidationException>();
            }
        }

        [Fact]
        public async Task Validate_will_not_allow_unknown_niscode()
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = "12345",
                    Buffer = ValidBuffer,
                    Description = ValidDescription
                });

                await act.Should().ThrowAsync<ValidationException>();
            }
        }

        [Fact]
        public async Task Validate_will_allow_known_niscode()
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                const string nisCode = "12345";

                await context.MunicipalityGeometries.AddAsync(new MunicipalityGeometry
                {
                    NisCode = nisCode,
                    Geometry = GeometryCollection.Empty
                });
                await context.SaveChangesAsync();

                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = nisCode,
                    Buffer = ValidBuffer,
                    Description = ValidDescription
                });

                await act.Should().NotThrowAsync<ValidationException>();
            }
        }

        [Theory]
        [MemberData(nameof(ValidDescriptionCases))]
        public async Task Validate_will_allow_valid_description(string givenDescription)
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                const string validNisCode = "12345";

                await context.MunicipalityGeometries.AddAsync(new MunicipalityGeometry
                {
                    NisCode = validNisCode,
                    Geometry = GeometryCollection.Empty
                });
                await context.SaveChangesAsync();

                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = validNisCode,
                    Buffer = ValidBuffer,
                    Description = givenDescription
                });

                await act.Should().NotThrowAsync<ValidationException>();
            }
        }

        public static IEnumerable<object[]> ValidDescriptionCases()
        {
            yield return new object[] { string.Empty };
            yield return new object[] { "description" };
            yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray()) };
        }

        [Theory]
        [MemberData(nameof(InvalidDescriptionCases))]
        public async Task Validate_will_not_allow_invalid_description(string givenDescription)
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                const string validNisCode = "12345";

                await context.MunicipalityGeometries.AddAsync(new MunicipalityGeometry
                {
                    NisCode = validNisCode,
                    Geometry = GeometryCollection.Empty
                });
                await context.SaveChangesAsync();

                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = validNisCode,
                    Buffer = ValidBuffer,
                    Description = givenDescription
                });

                await act.Should().ThrowAsync<ValidationException>();
            }
        }

        public static IEnumerable<object[]> InvalidDescriptionCases()
        {
            yield return new object[] { (string)null };
            yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray()) };
        }
    }
}
