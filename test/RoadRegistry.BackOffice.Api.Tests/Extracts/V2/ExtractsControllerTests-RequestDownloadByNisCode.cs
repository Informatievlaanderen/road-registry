namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentValidation;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Extracts;
using RoadRegistry.Sync.MunicipalityRegistry.Models;

public class DownloadExtractByNisCodeRequestValidatorTests
{
    private const int ValidBuffer = 50;
    private const string ValidDescription = "description";
    private readonly DbContextBuilder _dbContextBuilderFixture;

    public DownloadExtractByNisCodeRequestValidatorTests(DbContextBuilder dbContextBuilderFixture)
    {
        _dbContextBuilderFixture = dbContextBuilderFixture;
        //TODO-pr implement test
        //throw new NotImplementedException();
    }

    public static IEnumerable<object[]> InvalidDescriptionCases()
    {
        yield return new object[] { null };
        yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray()) };
    }
    //
    // [Fact]
    // public async Task Validate_will_allow_known_niscode()
    // {
    //     await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
    //     const string nisCode = "12345";
    //
    //     await context.Municipalities.AddAsync(new Municipality
    //     {
    //         MunicipalityId = "1",
    //         NisCode = nisCode,
    //         Status = MunicipalityStatus.Current,
    //         Geometry = GeometryCollection.Empty
    //     });
    //     await context.SaveChangesAsync();
    //
    //     var validator = new DownloadExtractByNisCodeRequestValidator(context);
    //
    //     var act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequest(nisCode, ValidBuffer, ValidDescription, false));
    //     await act.Should().NotThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [MemberData(nameof(ValidDescriptionCases))]
    // public async Task Validate_will_allow_valid_description(string givenDescription)
    // {
    //     await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
    //     const string validNisCode = "12345";
    //
    //     await context.Municipalities.AddAsync(new Municipality
    //     {
    //         MunicipalityId = "1",
    //         NisCode = validNisCode,
    //         Status = MunicipalityStatus.Current,
    //         Geometry = GeometryCollection.Empty
    //     });
    //     await context.SaveChangesAsync();
    //
    //     var validator = new DownloadExtractByNisCodeRequestValidator(context);
    //
    //     var act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequest(validNisCode, ValidBuffer, givenDescription, false));
    //     await act.Should().NotThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [InlineData("")]
    // [InlineData(null)]
    // public async Task Validate_will_not_allow_empty_niscodes(string givenNisCode)
    // {
    //     await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
    //     var validator = new DownloadExtractByNisCodeRequestValidator(context);
    //
    //     var act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequest(givenNisCode, ValidBuffer, ValidDescription, false));
    //     await act.Should().ThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [MemberData(nameof(InvalidDescriptionCases))]
    // public async Task Validate_will_not_allow_invalid_description(string givenDescription)
    // {
    //     await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
    //     const string validNisCode = "12345";
    //
    //     await context.Municipalities.AddAsync(new Municipality
    //     {
    //         MunicipalityId = "1",
    //         NisCode = validNisCode,
    //         Status = MunicipalityStatus.Current,
    //         Geometry = GeometryCollection.Empty
    //     });
    //     await context.SaveChangesAsync();
    //
    //     var validator = new DownloadExtractByNisCodeRequestValidator(context);
    //
    //     var act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequest(validNisCode, ValidBuffer, givenDescription, false));
    //     await act.Should().ThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [InlineData("invalid")]
    // [InlineData("1234")]
    // [InlineData("123456")]
    // [InlineData("1234A")]
    // public async Task Validate_will_not_allow_invalid_niscodes(string givenNisCode)
    // {
    //     await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
    //     var validator = new DownloadExtractByNisCodeRequestValidator(context);
    //
    //     var act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequest(
    //         givenNisCode,
    //         ValidBuffer,
    //         ValidDescription,
    //         false
    //     ));
    //     await act.Should().ThrowAsync<ValidationException>();
    // }
    //
    // [Fact]
    // public async Task Validate_will_not_allow_unknown_niscode()
    // {
    //     await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
    //     var validator = new DownloadExtractByNisCodeRequestValidator(context);
    //     var act = () => validator.ValidateAndThrowAsync(new DownloadExtractByNisCodeRequest("12345", ValidBuffer, ValidDescription, false));
    //     await act.Should().ThrowAsync<ValidationException>();
    // }
    //
    // public static IEnumerable<object[]> ValidDescriptionCases()
    // {
    //     yield return new object[] { string.Empty };
    //     yield return new object[] { "description" };
    //     yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray()) };
    // }
}
