namespace RoadRegistry.BackOffice.Api.Tests.Inwinning;

using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Api.Extracten;
using RoadRegistry.BackOffice.Api.Infrastructure.Options;
using RoadRegistry.BackOffice.Api.Inwinning;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.Sync.MunicipalityRegistry.Models;

public partial class InwinningControllerTests
{
    [Fact]
    public async Task WhenRequestInwinningExtract_ThenAcceptedResult()
    {
        // Arrange
        var locationResult = Fixture.Create<LocationResult>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RequestInwinningExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationResult);

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        var nisCode = "12345";
        municipalityContext.Municipalities.Add(new Municipality
        {
            MunicipalityId = Fixture.Create<string>(),
            NisCode = nisCode,
            Geometry = Polygon.Empty,
            Status = MunicipalityStatus.Current
        });
        await municipalityContext.SaveChangesAsync();

        var validator = new InwinningExtractDownloadaanvraagBodyValidator(municipalityContext);

        // Act
        var result = await Controller.RequestInwinningExtract(
            new InwinningExtractDownloadaanvraagBody(nisCode, Fixture.Create<string>(), true),
            validator,
            municipalityContext,
            new OptionsWrapper<InwinningOrganizationNisCodesOptions>(new InwinningOrganizationNisCodesOptions
            {
                {TestOrgCode, [nisCode]}
            }));

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        acceptedResult.Location.Should().Be(locationResult.Location.ToString());
        var extractDownloadaanvraagResponse = Assert.IsType<ExtractDownloadaanvraagResponse>(acceptedResult.Value);
        extractDownloadaanvraagResponse.DownloadId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task WhenRequestInwinningExtract_WithInvalidRequest_ThenValidationException()
    {
        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        var validator = new InwinningExtractDownloadaanvraagBodyValidator(municipalityContext);

        var nisCode = "12345";
        var act = () => Controller.RequestInwinningExtract(
            new InwinningExtractDownloadaanvraagBody(nisCode, default, default),
            validator,
            municipalityContext,
            new OptionsWrapper<InwinningOrganizationNisCodesOptions>(new InwinningOrganizationNisCodesOptions
            {
                {TestOrgCode, [nisCode]}
            }));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenRequestInwinningExtract_WithOrgNoAccess_ThenForbidden()
    {
        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        var validator = new InwinningExtractDownloadaanvraagBodyValidator(municipalityContext);

        var result = await Controller.RequestInwinningExtract(
            new InwinningExtractDownloadaanvraagBody("12345", default, default),
            validator,
            municipalityContext,
            new OptionsWrapper<InwinningOrganizationNisCodesOptions>(new InwinningOrganizationNisCodesOptions()));

        result.Should().BeOfType<ForbidResult>();
    }
}

public class InwinningExtractDownloadaanvraagBodyValidatorTests
{
    private const string ValidDescription = "description";
    private readonly DbContextBuilder _dbContextBuilderFixture;

    public InwinningExtractDownloadaanvraagBodyValidatorTests(DbContextBuilder dbContextBuilderFixture)
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

        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);

        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody(nisCode, ValidDescription, false));

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

        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);

        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody(validNisCode, givenDescription, false));

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task WhenNisCodeIsEmpty_ThenError(string givenNisCode)
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);

        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody(givenNisCode, ValidDescription, false));

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
    public async Task WhenNisCodeIsNotExpectedFormat_ThenNotFound(string givenNisCode)
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);

        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody(
            givenNisCode,
            ValidDescription,
            false
        ));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("NotFound");
    }

    [Fact]
    public async Task WhenNisCodeIsUnknown_ThenError()
    {
        await using var context = _dbContextBuilderFixture.CreateMunicipalityEventConsumerContext();
        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);
        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody("12345", ValidDescription, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("NotFound");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task WhenBeschrijvingIsEmpty_ThenError(string beschrijving)
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

        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);

        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody(validNisCode, beschrijving, false));

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        var error = result.Errors.First();
        error.ErrorCode.Should().Be("ExtractBeschrijvingIsRequired");
    }

    [Fact]
    public async Task WhenBeschrijvingTooLong_ThenError()
    {
        var beschrijving = new string(Enumerable.Repeat('a', ExtractDescription.MaxLength + 1).ToArray());

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

        var validator = new InwinningExtractDownloadaanvraagBodyValidator(context);

        var result = await validator.ValidateAsync(new InwinningExtractDownloadaanvraagBody(validNisCode, beschrijving, false));

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
