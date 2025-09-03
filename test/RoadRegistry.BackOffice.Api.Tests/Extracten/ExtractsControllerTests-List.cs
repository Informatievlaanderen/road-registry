namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using Abstractions.Extracts.V2;
using Api.Extracten;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ExtractListItem = Abstractions.Extracts.V2.ExtractListItem;

public partial class ExtractsControllerTests
{
    [Theory]
    [InlineData(false)]
    public async Task WhenGettingAllExtracts_ThenNoFilter(bool? eigenExtracten)
    {
        // Arrange
        var extractListResponse = new ExtractListResponse
        {
            Items = [new ExtractListItem()]
        };

        Mediator
            .Setup(x => x.Send(new ExtractListRequest(null, 0, 100), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractListResponse);

        // Act
        var result = await Controller.ListExtracten(
            eigenExtracten: eigenExtracten,
            page: "0");

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<ExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(extractListResponse.Items.Count);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(null)]
    public async Task WhenGettingOnlyOwnExtracts_ThenFilteredByOrganizationCode(bool? eigenExtracten)
    {
        // Arrange
        var extractListResponse = new ExtractListResponse
        {
            Items = [new ExtractListItem()]
        };

        Mediator
            .Setup(x => x.Send(new ExtractListRequest(TestOrgCode, 0, 100), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractListResponse);

        // Act
        var result = await Controller.ListExtracten(
            eigenExtracten: eigenExtracten,
            page: "0");

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<ExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(extractListResponse.Items.Count);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    public async Task WhenGettingExtracts_WithInvalidPage_ThenValidationException(string page)
    {
        var act = () => Controller.ListExtracten(
            page: page);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
