namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using Abstractions.Extracts.V2;
using Api.Extracten;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
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
            page: 0);

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
            page: 0);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<ExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(extractListResponse.Items.Count);
    }

    [Theory]
    [InlineData(-1)]
    public async Task WhenGettingExtracts_WithInvalidPage_ThenValidationException(int page)
    {
        var act = () => Controller.ListExtracten(
            page: page);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // The list answers what the detail screen answers: a delivery that was approved and then refused by the road
    // network reads as approved to the organization that uploaded it, and the two screens never disagree.
    [Fact]
    public async Task WhenADeliveryFailedProcessing_ThenItIsListedAsAcceptedForTheUploader()
    {
        // Arrange
        Mediator
            .Setup(x => x.Send(new ExtractListRequest(TestOrgCode, 0, 100), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractListResponse
            {
                Items = [new ExtractListItem { UploadStatus = nameof(RoadRegistry.Extracts.Schema.ExtractUploadStatus.ProcessingFailed) }]
            });

        // Act
        var result = await Controller.ListExtracten(eigenExtracten: true, page: 0);

        // Assert
        var responseObject = Assert.IsType<ExtractsListResponse>(Assert.IsType<OkObjectResult>(result).Value);

        responseObject.Items.Single().UploadStatus
            .Should().Be(nameof(RoadRegistry.Extracts.Schema.ExtractUploadStatus.Accepted));
    }
}
