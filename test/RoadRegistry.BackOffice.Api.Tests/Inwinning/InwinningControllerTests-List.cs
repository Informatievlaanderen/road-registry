namespace RoadRegistry.BackOffice.Api.Tests.Inwinning;

using System.Security.Claims;
using Abstractions.Extracts.V2;
using Api.Infrastructure.Authentication;
using Api.Inwinning;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ExtractListItem = Abstractions.Extracts.V2.ExtractListItem;

public partial class InwinningControllerTests
{
    [Fact]
    public async Task WhenListingInwinningExtracten_ThenFilteredByOrganizationCode()
    {
        // Arrange
        Mediator
            .Setup(x => x.Send(new InwinningExtractListRequest(TestOrgCode), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractListResponse { Items = [new ExtractListItem()] });

        // Act
        var result = await Controller.ListInwinningExtracten();

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<InwinningExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task WhenListingInwinningExtractenAsDigitaalVlaanderenAdmin_ThenNoFilter()
    {
        // Arrange
        var controller = BuildController(
            new Claim("vo_orgcode", OrganizationOvoCode.DigitaalVlaanderen),
            new Claim(RoadRegistryClaim.ClaimType, RoadRegistryClaim.ConvertRoleToClaimValue(RoadRegistryRoles.Admin)));

        Mediator
            .Setup(x => x.Send(new InwinningExtractListRequest(null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractListResponse { Items = [new ExtractListItem(), new ExtractListItem()] });

        // Act
        var result = await controller.ListInwinningExtracten();

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<InwinningExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(2);
    }

    [Fact]
    public async Task WhenListingInwinningExtractenAsDigitaalVlaanderenAdminByOvoCodeClaim_ThenNoFilter()
    {
        // Arrange
        // ACM/IDM hands the OVO-code over in 'vo_ovocode' for some clients and in 'vo_orgcode' for others.
        var controller = BuildController(
            new Claim("vo_ovocode", OrganizationOvoCode.DigitaalVlaanderen),
            new Claim("vo_orgcode", TestOrgCode),
            new Claim(RoadRegistryClaim.ClaimType, RoadRegistryClaim.ConvertRoleToClaimValue(RoadRegistryRoles.Admin)));

        Mediator
            .Setup(x => x.Send(new InwinningExtractListRequest(null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractListResponse { Items = [new ExtractListItem(), new ExtractListItem()] });

        // Act
        var result = await controller.ListInwinningExtracten();

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<InwinningExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(2);
    }

    [Fact]
    public async Task WhenListingInwinningExtractenAsDigitaalVlaanderenEditor_ThenFilteredByOrganizationCode()
    {
        // Arrange
        var controller = BuildController(
            new Claim("vo_orgcode", OrganizationOvoCode.DigitaalVlaanderen),
            new Claim(RoadRegistryClaim.ClaimType, RoadRegistryClaim.ConvertRoleToClaimValue(RoadRegistryRoles.Editor)));

        Mediator
            .Setup(x => x.Send(new InwinningExtractListRequest(OrganizationOvoCode.DigitaalVlaanderen), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractListResponse { Items = [new ExtractListItem()] });

        // Act
        var result = await controller.ListInwinningExtracten();

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<InwinningExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task WhenListingInwinningExtractenAsAdminOfAnotherOrganization_ThenFilteredByOrganizationCode()
    {
        // Arrange
        var controller = BuildController(
            new Claim("vo_orgcode", TestOrgCode),
            new Claim(RoadRegistryClaim.ClaimType, RoadRegistryClaim.ConvertRoleToClaimValue(RoadRegistryRoles.Admin)));

        Mediator
            .Setup(x => x.Send(new InwinningExtractListRequest(TestOrgCode), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractListResponse { Items = [new ExtractListItem()] });

        // Act
        var result = await controller.ListInwinningExtracten();

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<InwinningExtractsListResponse>(okObjectResult.Value);

        responseObject.Items.Count.Should().Be(1);
    }
}
