namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Collections.Generic;
using System.Linq;
using Api.Extracten;
using AutoFixture;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FeatureToggles;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenRequestExtractByContour_ThenAcceptedResult()
    {
        // Arrange
        var locationResult = Fixture.Create<LocationResult>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationResult);

        var validator = new ExtractDownloadaanvraagPerContourBodyValidator();

        // Act
        var result = await Controller.ExtractDownloadaanvraagPerContour(
            new ExtractDownloadaanvraagPerContourBody(Polygon.Empty.AsText(), Fixture.Create<string>(), true, Fixture.Create<string>()),
            validator,
            new UseDomainV2FeatureToggle(false));

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        acceptedResult.Location.Should().Be(locationResult.Location.ToString());
        var extractDownloadaanvraagResponse = Assert.IsType<ExtractDownloadaanvraagResponse>(acceptedResult.Value);
        extractDownloadaanvraagResponse.DownloadId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WhenRequestExtractByContour_WithInvalidRequest_ThenValidationException()
    {
        var validator = new ExtractDownloadaanvraagPerContourBodyValidator();

        var act = () => Controller.ExtractDownloadaanvraagPerContour(
            new ExtractDownloadaanvraagPerContourBody(default, default, default, default),
            validator,
            new UseDomainV2FeatureToggle(false));

        await act.Should().ThrowAsync<ValidationException>();
    }
}

public class ExtractDownloadaanvraagPerContourBodyValidatorTests
{
    private const string ValidContour = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))";
    private const string ValidDescription = "description";
    private readonly ExtractDownloadaanvraagPerContourBodyValidator _validator;

    public ExtractDownloadaanvraagPerContourBodyValidatorTests()
    {
        _validator = new ExtractDownloadaanvraagPerContourBodyValidator();
    }

    public static IEnumerable<object[]> ValidDescriptionCases()
    {
        yield return ["description"];
        yield return [new string(Enumerable.Repeat('a', ExtractDescription.MaxLength).ToArray())];
    }

    [Theory]
    [MemberData(nameof(ValidDescriptionCases))]
    public async Task WhenDescriptionIsValid_ThenNone(string givenDescription)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(ValidContour, givenDescription, false, null));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task WhenGeometryIsValid_ThenNone()
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(ValidContour, ValidDescription, false, null));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WhenContourIsEmpty_ThenError(string contour)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(contour, ValidDescription, false, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractContourIsRequired");
    }

    [Theory]
    [InlineData("invalid")]
    // A sliver whose ring crosses itself around (194233.64, 206922.22): the per-contour endpoint already turned this
    // one away, the shape file endpoint did not, and the extract it produced brought down the lambda.
    [InlineData("MULTIPOLYGON (((194477.00366637472 206971.83339355365, 194469.6805155593 206969.91542548305, 194452.85470475757 206975.14624749395, 193640.4208660975 206778.9904220851, 193345.57686541631 206711.33845741072, 193326.39718470967 206705.75891393243, 193324.3048559053 206719.18469042706, 193409.567254683 206737.66692819892, 193820.53550400632 206827.11398458539, 194105.96402506795 206891.80181678684, 194197.32904952508 206912.02766189564, 194470.37795849424 206988.65920435538, 194475.26005903777 206978.19756033356, 194477.00366637472 206971.83339355365)))")]
    public async Task WhenContourIsInvalid_ThenError(string contour)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(contour, ValidDescription, false, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractContourInvalid");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WhenBeschrijvingIsEmpty_ThenError(string beschrijving)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(ValidContour, beschrijving, false, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingIsRequired");
    }

    [Fact]
    public async Task WhenBeschrijvingTooLong_ThenError()
    {
        var beschrijving = new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray());
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(ValidContour, beschrijving, false, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingTooLong");
    }
}
