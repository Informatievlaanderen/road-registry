namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Api.Extracten;
using AutoFixture;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FeatureToggles;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenRequestExtractByFile_ThenAcceptedResult()
    {
        // Arrange
        var locationResult = Fixture.Create<LocationResult>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationResult);

        var validator = new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8);
        var shpFileContourReader = new Mock<IExtractShapefileContourReader>();
        shpFileContourReader
            .Setup(x => x.Read(It.IsAny<Stream>()))
            .Returns(Polygon.Empty);

        var bestanden = new FormFileCollection
        {
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.shp", "application/octet-stream", CancellationToken.None),
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.prj", "application/octet-stream", CancellationToken.None)
        };

        // Act
        var result = await Controller.ExtractDownloadaanvraagPerBestand(
            new ExtractDownloadaanvraagPerBestandBody(Fixture.Create<string>(), bestanden, true),
            validator,
            shpFileContourReader.Object,
            new UseDomainV2FeatureToggle(false));

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        acceptedResult.Location.Should().Be(locationResult.Location.ToString());
        var extractDownloadaanvraagResponse = Assert.IsType<ExtractDownloadaanvraagResponse>(acceptedResult.Value);
        extractDownloadaanvraagResponse.DownloadId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WhenRequestExtractByFile_WithInvalidRequest_ThenValidationException()
    {
        var validator = new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8);

        var act = () => Controller.ExtractDownloadaanvraagPerBestand(
            new ExtractDownloadaanvraagPerBestandBody(default, new FormFileCollection(), default),
            validator,
            Mock.Of<IExtractShapefileContourReader>(),
            new UseDomainV2FeatureToggle(false));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenRequestExtractByFile_WithMissingFiles_ThenValidationException()
    {
        var validator = new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8);

        var act = () => Controller.ExtractDownloadaanvraagPerBestand(
            new ExtractDownloadaanvraagPerBestandBody(Fixture.Create<string>(), new FormFileCollection(), default),
            validator,
            Mock.Of<IExtractShapefileContourReader>(),
            new UseDomainV2FeatureToggle(false));

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        ex.Errors.Count().Should().Be(1);
        ex.Errors.Should().ContainSingle(e => e.ErrorCode == "BestandVerplicht");
    }
}

public class ExtractDownloadaanvraagPerBestandValidatorTests : IAsyncLifetime
{
    private const string ValidDescription = "description";
    private readonly ExtractDownloadaanvraagPerBestandValidator _validator;
    private ExtractDownloadaanvraagPerBestandItem _prjFilePolygon;
    private ExtractDownloadaanvraagPerBestandItem _shpFilePolygon;

    public ExtractDownloadaanvraagPerBestandValidatorTests()
    {
        _validator = new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8);
    }

    public async Task DisposeAsync()
    {
        await _prjFilePolygon.ReadStream.DisposeAsync();
        await _shpFilePolygon.ReadStream.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        _prjFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.prj");
        _shpFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.shp");
    }

    private async Task<ExtractDownloadaanvraagPerBestandItem> GetDownloadExtractByFileRequestItemFromResource(string name)
    {
        return new ExtractDownloadaanvraagPerBestandItem(name, await EmbeddedResourceReader.ReadAsync(name), ContentType.Parse("application/octet-stream"));
    }

    [Theory]
    [MemberData(nameof(ValidDescriptionCases))]
    public async Task WhenBescrijvingIsValid_ThenNone(string givenDescription)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerBestand(_shpFilePolygon, _prjFilePolygon, givenDescription, false));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task WhenBestandenIsValid_ThenNone()
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerBestand(_shpFilePolygon, _prjFilePolygon, ValidDescription, false));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task WhenProjectionFileIsInvalid_ThenError()
    {
        var prjFileInvalid = await GetDownloadExtractByFileRequestItemFromResource("invalid.prj");
        await using (prjFileInvalid.ReadStream)
        {
            var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerBestand(_shpFilePolygon, prjFileInvalid, ValidDescription, false));

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            var error = result.Errors.First();
            error.ErrorCode.Should().Be("ExtractProjectionInvalid");
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WhenBeschrijvingIsEmpty_ThenError(string bescrijving)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerBestand(_shpFilePolygon, _prjFilePolygon, bescrijving, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingIsRequired");
    }

    [Fact]
    public async Task WhenBeschrijvingTooLong_ThenError()
    {
        var beschrijving = new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray());
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerBestand(_shpFilePolygon, _prjFilePolygon, beschrijving, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingTooLong");
    }

    public static IEnumerable<object[]> ValidDescriptionCases()
    {
        yield return ["description"];
        yield return [new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray())];
    }
}
