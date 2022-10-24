namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using BackOffice.Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using FluentValidation;
using Handlers.Extracts;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.IO;

public class DownloadExtractByFileRequestValidatorTests
{
    private const int ValidBuffer = 50;
    private const string ValidDescription = "description";
    private readonly DownloadExtractByFileRequestValidator _validator;

    private static readonly DownloadExtractByFileRequestItem ShpFile = new ("filename.shp", Stream.Null, ContentType.Parse("application/octet-stream"));
    private static readonly DownloadExtractByFileRequestItem ShxFile = new ("filename.shx", Stream.Null, ContentType.Parse("application/octet-stream"));
    private static readonly DownloadExtractByFileRequestItem PrjFile = new ("filename.prj", Stream.Null, ContentType.Parse("application/octet-stream"));

    public DownloadExtractByFileRequestValidatorTests()
    {
        _validator = new DownloadExtractByFileRequestValidator();
    }

    public static IEnumerable<object[]> InvalidDescriptionCases()
    {
        yield return new object[] { null };
        yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray()) };
    }



    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(100)]
    public async Task Validate_will_allow_valid_buffer(int givenBuffer)
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(ShpFile, ShxFile, PrjFile, givenBuffer, ValidDescription));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Theory]
    [MemberData(nameof(ValidDescriptionCases))]
    public async Task Validate_will_allow_valid_description(string givenDescription)
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(ShpFile, ShxFile, PrjFile, ValidBuffer, givenDescription));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_will_allow_valid_geometry()
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(ShpFile, ShxFile, PrjFile, ValidBuffer, ValidDescription));

        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task Validate_will_not_allow_invalid_buffer(int givenBuffer)
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(ShpFile, ShxFile, PrjFile, givenBuffer, ValidDescription));
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [MemberData(nameof(InvalidDescriptionCases))]
    public async Task Validate_will_not_allow_invalid_description(string givenDescription)
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(ShpFile, ShxFile, PrjFile, ValidBuffer, givenDescription));
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_will_not_allow_invalid_geometry()
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(ShpFile, ShxFile, PrjFile, ValidBuffer, ValidDescription));
        await act.Should().ThrowAsync<ValidationException>();
    }

    public static IEnumerable<object[]> ValidDescriptionCases()
    {
        yield return new object[] { string.Empty };
        yield return new object[] { "description" };
        yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray()) };
    }
}
