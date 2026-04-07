namespace RoadRegistry.BackOffice.Api.Tests.Inwinning;

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Api.Inwinning;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Sync.MunicipalityRegistry.Models;

public partial class InwinningsstatusControllerTests
{
    [Fact]
    public async Task GivenCompletedInwinningszone_WhenGettingGemeenteInwinningsstatus_ThenCompleet()
    {
        // Arrange
        var niscode = string.Join(string.Empty, Fixture.CreateMany<char>(5));

        _extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            NisCode = niscode,
            Completed = true,
            Contour = Polygon.Empty,
            DownloadId = Fixture.Create<Guid>(),
            Operator = Fixture.Create<string>()
        });
        await _extractsDbContext.SaveChangesAsync();

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();

        // Act
        var result = await Controller.GetGemeenteInwinningsstatus(
            niscode,
            _extractsDbContext,
            municipalityContext,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<GemeenteInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Compleet);
    }

    [Fact]
    public async Task GivenNotCompletedInwinningszone_WhenGettingGemeenteInwinningsstatus_ThenLocked()
    {
        // Arrange
        var niscode = string.Join(string.Empty, Fixture.CreateMany<char>(5));

        _extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            NisCode = niscode,
            Completed = false,
            Contour = Polygon.Empty,
            DownloadId = Fixture.Create<Guid>(),
            Operator = Fixture.Create<string>()
        });
        await _extractsDbContext.SaveChangesAsync();

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();

        // Act
        var result = await Controller.GetGemeenteInwinningsstatus(
            niscode,
            _extractsDbContext,
            municipalityContext,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<GemeenteInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.Locked);
    }

    [Fact]
    public async Task GivenNoInwinningszoneButExistingMunicipality_WhenGettingGemeenteInwinningsstatus_ThenNietGestart()
    {
        // Arrange
        var niscode = string.Join(string.Empty, Fixture.CreateMany<char>(5));

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        municipalityContext.Municipalities.Add(new Municipality
        {
            MunicipalityId = Fixture.Create<string>(),
            NisCode = niscode,
            Geometry = Polygon.Empty,
            Status = MunicipalityStatus.Current
        });
        await municipalityContext.SaveChangesAsync();

        // Act
        var result = await Controller.GetGemeenteInwinningsstatus(
            niscode,
            _extractsDbContext,
            municipalityContext,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<GemeenteInwinningsstatus>(okObjectResult.Value);

        responseObject.Inwinningsstatus.Should().Be(Inwinningsstatus.NietGestart);
    }

    [Fact]
    public async Task GivenNoInwinningszoneAndNoMunicipality_WhenGettingGemeenteInwinningsstatus_ThenNotFound()
    {
        // Arrange
        var niscode = string.Join(string.Empty, Fixture.CreateMany<char>(5));

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();

        // Act
        var result = await Controller.GetGemeenteInwinningsstatus(
            niscode,
            _extractsDbContext,
            municipalityContext,
            CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
