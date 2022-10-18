namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using BackOffice.Abstractions.Extracts;
using FluentAssertions;
using FluentValidation;
using Handlers.Extracts;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.IO;

public class DownloadExtractRequestValidatorTests
{
    public DownloadExtractRequestValidatorTests()
    {
        _validator = new DownloadExtractRequestValidator(
            new WKTReader(),
            new NullLogger<DownloadExtractRequestValidator>());
    }

    private readonly DownloadExtractRequestValidator _validator;

    [Fact]
    public async Task Validate_will_allow_valid_geometry()
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequest("request id", "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))"));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_will_not_allow_empty_geometry(string givenContour)
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequest("request id", givenContour));
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_will_not_allow_empty_request_id(string givenRequestId)
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequest(givenRequestId, "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)))"));
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Validate_will_not_allow_invalid_geometry()
    {
        var act = () => _validator.ValidateAndThrowAsync(new DownloadExtractRequest("request id", "invalid"));
        await act.Should().ThrowAsync<ValidationException>();
    }
}
