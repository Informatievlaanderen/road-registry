namespace RoadRegistry.BackOffice.Api.Extracts
{
    using System;
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
        private readonly SqlServer _sqlServerFixture;

        public DownloadExtractByNisCodeRequestBodyValidatorTests(SqlServer sqlServerFixture)
        {
            _sqlServerFixture = sqlServerFixture;
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_will_not_allow_empty_values(string givenNisCode)
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = givenNisCode
                });
                await act.Should().ThrowAsync<ValidationException>();
            }
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("1234")]
        [InlineData("123456")]
        [InlineData("1234A")]
        public async Task Validate_will_not_allow_invalid_values(string givenNisCode)
        {
            using (var context = await _sqlServerFixture.CreateEmptyEditorContextAsync(await _sqlServerFixture.CreateDatabaseAsync()))
            {
                var logger = new NullLogger<DownloadExtractByNisCodeRequestBodyValidator>();
                var validator = new DownloadExtractByNisCodeRequestBodyValidator(context, logger);

                Func<Task> act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequestBody
                {
                    NisCode = givenNisCode
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
                    NisCode = "12345"
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
                    NisCode = nisCode
                });

                await act.Should().NotThrowAsync<ValidationException>();
            }
        }
    }
}
