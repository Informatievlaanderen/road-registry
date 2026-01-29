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
    public async Task WhenBeschrijvingIsEmpty_ThenError(string bescrijving)
    {
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(ValidContour, bescrijving, false, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingIsRequired");
    }

    [Fact]
    public async Task WhenBeschrijvingTooLong_ThenError()
    {
        var bescrijving = new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray());
        var result = await _validator.ValidateAsync(new ExtractDownloadaanvraagPerContourBody(ValidContour, bescrijving, false, null));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingTooLong");
    }
}
