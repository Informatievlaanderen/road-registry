namespace RoadRegistry.BackOffice.Api.Tests.Extracts.V2;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using FluentValidation;
using RoadRegistry.BackOffice.Abstractions.Extracts;
using RoadRegistry.BackOffice.Handlers.Extracts;

public class DownloadExtractByFileRequestValidatorTests : IAsyncLifetime
{
    private const int ValidBuffer = 50;
    private const string ValidDescription = "description";
    private readonly DownloadExtractByFileRequestValidator _validator;
    private DownloadExtractByFileRequestItem _prjFilePolygon;
    private DownloadExtractByFileRequestItem _shpFilePolygon;

    public DownloadExtractByFileRequestValidatorTests()
    {
        _validator = new DownloadExtractByFileRequestValidator(Encoding.UTF8);
        //TODO-pr implement test
        throw new NotImplementedException();
    }

    public async Task DisposeAsync()
    {
        await _prjFilePolygon.ReadStream.DisposeAsync();
        await _shpFilePolygon.ReadStream.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        // _prjFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.prj");
        // _shpFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.shp");
    }
    //
    // private async Task<DownloadExtractByFileRequestItem> GetDownloadExtractByFileRequestItemFromResource(string name)
    // {
    //     return new DownloadExtractByFileRequestItem(name, await EmbeddedResourceReader.ReadAsync(name), ContentType.Parse("application/octet-stream"));
    // }
    //
    // public static IEnumerable<object[]> InvalidDescriptionCases()
    // {
    //     yield return new object[] { null };
    //     yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray()) };
    // }
    //
    // [Theory]
    // [InlineData(0)]
    // [InlineData(1)]
    // [InlineData(99)]
    // [InlineData(100)]
    // public async Task Validate_will_allow_valid_buffer(int givenBuffer)
    // {
    //     var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(_shpFilePolygon, _prjFilePolygon, givenBuffer, ValidDescription, false));
    //     await act.Should().NotThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [MemberData(nameof(ValidDescriptionCases))]
    // public async Task Validate_will_allow_valid_description(string givenDescription)
    // {
    //     var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(_shpFilePolygon, _prjFilePolygon, ValidBuffer, givenDescription, false));
    //     await act.Should().NotThrowAsync<ValidationException>();
    // }
    //
    // [Fact]
    // public async Task Validate_will_allow_valid_geometry()
    // {
    //     var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(_shpFilePolygon, _prjFilePolygon, ValidBuffer, ValidDescription, false));
    //     await act.Should().NotThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [InlineData(-1)]
    // [InlineData(101)]
    // [InlineData(int.MinValue)]
    // [InlineData(int.MaxValue)]
    // public async Task Validate_will_not_allow_invalid_buffer(int givenBuffer)
    // {
    //     var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(_shpFilePolygon, _prjFilePolygon, givenBuffer, ValidDescription, false));
    //     await act.Should().ThrowAsync<ValidationException>();
    // }
    //
    // [Theory]
    // [MemberData(nameof(InvalidDescriptionCases))]
    // public async Task Validate_will_not_allow_invalid_description(string givenDescription)
    // {
    //     var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(_shpFilePolygon, _prjFilePolygon, ValidBuffer, givenDescription, false));
    //     await act.Should().ThrowAsync<ValidationException>();
    // }
    //
    // [Fact]
    // public async Task Validate_will_not_allow_invalid_projection()
    // {
    //     var prjFileInvalid = await GetDownloadExtractByFileRequestItemFromResource("invalid.prj");
    //     using (prjFileInvalid.ReadStream)
    //     {
    //         var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractByFileRequest(_shpFilePolygon, prjFileInvalid, ValidBuffer, ValidDescription, false));
    //         await act.Should().ThrowAsync<ValidationException>();
    //     }
    // }
    //
    // public static IEnumerable<object[]> ValidDescriptionCases()
    // {
    //     yield return new object[] { string.Empty };
    //     yield return new object[] { "description" };
    //     yield return new object[] { new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray()) };
    // }
}
