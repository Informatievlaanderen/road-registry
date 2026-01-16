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
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using Sync.MunicipalityRegistry.Models;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenRequestExtractByNisCode_ThenAcceptedResult()
    {
        // Arrange
        var locationResult = Fixture.Create<LocationResult>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationResult);

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        municipalityContext.Municipalities.Add(new Municipality
        {
            MunicipalityId = Fixture.Create<string>(),
            NisCode = "12345",
            Geometry = Polygon.Empty,
            Status = MunicipalityStatus.Current
        });
        await municipalityContext.SaveChangesAsync();

        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(municipalityContext);

        // Act
        var result = await Controller.ExtractDownloadaanvraagPerNisCode(
            new ExtractDownloadaanvraagPerNisCodeBody("12345", Fixture.Create<string>(), true),
            validator,
            municipalityContext,
            new UseDomainV2FeatureToggle(false));

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        acceptedResult.Location.Should().Be(locationResult.Location.ToString());
        var extractDownloadaanvraagResponse = Assert.IsType<ExtractDownloadaanvraagResponse>(acceptedResult.Value);
        extractDownloadaanvraagResponse.DownloadId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WhenRequestExtractByNisCode_WithInvalidRequest_ThenValidationException()
    {
        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(municipalityContext);

        var act = () => Controller.ExtractDownloadaanvraagPerNisCode(
            new ExtractDownloadaanvraagPerNisCodeBody(default, default, default),
            validator,
            municipalityContext,
            new UseDomainV2FeatureToggle(false));

        await act.Should().ThrowAsync<ValidationException>();
    }
}

public class ExtractDownloadaanvraagPerNisCodeBodyValidatorTests
{
    private const string ValidDescription = "description";
    private readonly DbContextBuilder _dbContextBuilderFixture;

    public ExtractDownloadaanvraagPerNisCodeBodyValidatorTests(DbContextBuilder dbContextBuilderFixture)
    {
        _dbContextBuilderFixture = dbContextBuilderFixture;
    }

    [Fact]
    public async Task WhenNisCodeIsValid_ThenNone()
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        const string nisCode = "12345";

        await context.Municipalities.AddAsync(new Municipality
        {
            MunicipalityId = "1",
            NisCode = nisCode,
            Status = MunicipalityStatus.Current,
            Geometry = Polygon.Empty
        });
        await context.SaveChangesAsync();

        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);

        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody(nisCode, ValidDescription, false));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(ValidDescriptionCases))]
    public async Task WhenBeschrijvingIsValid_ThenNone(string givenDescription)
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        const string validNisCode = "12345";

        await context.Municipalities.AddAsync(new Municipality
        {
            MunicipalityId = "1",
            NisCode = validNisCode,
            Status = MunicipalityStatus.Current,
            Geometry = Polygon.Empty
        });
        await context.SaveChangesAsync();

        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);

        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody(validNisCode, givenDescription, false));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task WhenNisCodeIsEmpty_ThenError(string givenNisCode)
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);

        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody(givenNisCode, ValidDescription, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractNisCodeIsRequired");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("1234A")]
    public async Task WhenNisCodeIsInvalid_ThenError(string givenNisCode)
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);

        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody(
            givenNisCode,
            ValidDescription,
            false
        ));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractNisCodeInvalid");
    }

    [Fact]
    public async Task WhenNisCodeIsUnknown_ThenError()
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);
        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody("12345", ValidDescription, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("NotFound");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WhenBeschrijvingIsEmpty_ThenError(string bescrijving)
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        const string validNisCode = "12345";

        await context.Municipalities.AddAsync(new Municipality
        {
            MunicipalityId = "1",
            NisCode = validNisCode,
            Status = MunicipalityStatus.Current,
            Geometry = Polygon.Empty
        });
        await context.SaveChangesAsync();

        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);

        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody(validNisCode, bescrijving, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingIsRequired");
    }

    [Fact]
    public async Task WhenBeschrijvingTooLong_ThenError()
    {
        var bescrijving = new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray());

        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        const string validNisCode = "12345";

        await context.Municipalities.AddAsync(new Municipality
        {
            MunicipalityId = "1",
            NisCode = validNisCode,
            Status = MunicipalityStatus.Current,
            Geometry = Polygon.Empty
        });
        await context.SaveChangesAsync();

        var validator = new ExtractDownloadaanvraagPerNisCodeBodyValidator(context);

        var result = await validator.ValidateAsync(new ExtractDownloadaanvraagPerNisCodeBody(validNisCode, bescrijving, false));

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
