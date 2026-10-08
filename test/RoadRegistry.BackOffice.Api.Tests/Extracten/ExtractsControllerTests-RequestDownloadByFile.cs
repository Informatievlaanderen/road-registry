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
using NetTopologySuite.IO;

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
            .Setup(x => x.Read(It.IsAny<Stream>(), It.IsAny<GeometryFactory>()))
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
    public async Task WhenRequestExtractByFile_WithSelfIntersectingContour_ThenValidationException()
    {
        // A sliver drawn along a road whose ring crosses itself around (194233.64, 206922.22). The shape file reads
        // fine, so nothing stopped it before: it was stored and brought down the lambda assembling the extract, with
        // SQL Server refusing to run STIntersects on an invalid instance.
        const string selfIntersectingContour = "MULTIPOLYGON (((194477.00366637472 206971.83339355365, 194469.6805155593 206969.91542548305, 194452.85470475757 206975.14624749395, 193640.4208660975 206778.9904220851, 193345.57686541631 206711.33845741072, 193326.39718470967 206705.75891393243, 193324.3048559053 206719.18469042706, 193409.567254683 206737.66692819892, 193820.53550400632 206827.11398458539, 194105.96402506795 206891.80181678684, 194197.32904952508 206912.02766189564, 194470.37795849424 206988.65920435538, 194475.26005903777 206978.19756033356, 194477.00366637472 206971.83339355365)))";

        var validator = new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8);
        var shpFileContourReader = new Mock<IExtractShapefileContourReader>();
        shpFileContourReader
            .Setup(x => x.Read(It.IsAny<Stream>(), It.IsAny<GeometryFactory>()))
            .Returns(new WKTReader().Read(selfIntersectingContour));

        var bestanden = new FormFileCollection
        {
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.shp", "application/octet-stream", CancellationToken.None),
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.prj", "application/octet-stream", CancellationToken.None)
        };

        var act = () => Controller.ExtractDownloadaanvraagPerBestand(
            new ExtractDownloadaanvraagPerBestandBody(Fixture.Create<string>(), bestanden, true),
            validator,
            shpFileContourReader.Object,
            new UseDomainV2FeatureToggle(false));

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        ex.Errors.Should().ContainSingle(e => e.ErrorCode == "ExtractContourInvalid");
        Mediator.Verify(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenRequestExtractByFile_WithContourLargerThanTheWktMaximum_ThenAcceptedResult()
    {
        // The 200km2 limit belongs to the contour taken as WKT. The contours GRB sends in as a shape file run well
        // past it - a stored one measures close to 300km2 - so holding this endpoint to that limit would turn away
        // exactly the requests it exists for. Only the geometry is checked here.
        var locationResult = Fixture.Create<LocationResult>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationResult);

        var validator = new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8);
        var shpFileContourReader = new Mock<IExtractShapefileContourReader>();
        shpFileContourReader
            .Setup(x => x.Read(It.IsAny<Stream>(), It.IsAny<GeometryFactory>()))
            .Returns(new WKTReader().Read("POLYGON ((0 0, 0 20000, 20000 20000, 20000 0, 0 0))")); // 400km2, twice the WKT maximum

        var bestanden = new FormFileCollection
        {
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.shp", "application/octet-stream", CancellationToken.None),
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.prj", "application/octet-stream", CancellationToken.None)
        };

        var result = await Controller.ExtractDownloadaanvraagPerBestand(
            new ExtractDownloadaanvraagPerBestandBody(Fixture.Create<string>(), bestanden, true),
            validator,
            shpFileContourReader.Object,
            new UseDomainV2FeatureToggle(false));

        Assert.IsType<AcceptedResult>(result);
        Mediator.Verify(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task WhenbeschrijvingIsValid_ThenNone(string givenDescription)
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
    public async Task WhenBeschrijvingIsEmpty_ThenError(string beschrijving)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerBestand(_shpFilePolygon, _prjFilePolygon, beschrijving, false));

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
