namespace RoadRegistry.BackOffice.Api.Tests.Inwinning;

using System.Linq;
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

        var extractRequestId = Fixture.Create<string>();
        var extractRequestedAt = DateTimeOffset.UtcNow;
        var downloadId = Fixture.Create<Guid>();
        _extractsDbContext.ExtractRequests.Add(new ExtractRequest
        {
            ExtractRequestId = extractRequestId,
            ExternalRequestId = $"INWINNING_{niscode}",
            Description = Fixture.Create<string>(),
            OrganizationCode = Fixture.Create<string>(),
            RequestedOn = extractRequestedAt,
            CurrentDownloadId = downloadId
        });
        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Closed = true,
            ClosedOn = extractRequestedAt.AddHours(3).AddMilliseconds(1),
            IsInformative = false,
            Contour = Polygon.Empty,
            Status = ExtractDownloadStatus.Available,
            RequestedOn = extractRequestedAt,
            ZipArchiveWriterVersion = Fixture.Create<string>()
        });
        var uploadId = Fixture.Create<Guid>();
        _extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = uploadId,
            DownloadId = downloadId,
            Status = ExtractUploadStatus.Accepted,
            UploadedOn = extractRequestedAt.AddHours(3),
            TicketId = Fixture.Create<Guid>()
        });
        _extractsDbContext.ExtractUploadStatusHistory.Add(new ExtractUploadStatusHistory
        {
            UploadId = uploadId,
            Status = ExtractUploadStatus.Processing,
            Timestamp = extractRequestedAt.AddHours(1)
        });
        _extractsDbContext.ExtractUploadStatusHistory.Add(new ExtractUploadStatusHistory
        {
            UploadId = uploadId,
            Status = ExtractUploadStatus.AutomaticValidationSucceeded,
            Timestamp = extractRequestedAt.AddHours(2)
        });
        _extractsDbContext.ExtractUploadStatusHistory.Add(new ExtractUploadStatusHistory
        {
            UploadId = uploadId,
            Status = ExtractUploadStatus.Accepted,
            Timestamp = extractRequestedAt.AddHours(3)
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
        responseObject.HistoriekExtracten.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GivenCompletedInwinningszone_WhenGettingGemeenteInwinningsstatus_ThenExcludeInformativeExtracts()
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

        var informatiefExtractRequestId = Fixture.Create<string>();
        _extractsDbContext.ExtractRequests.Add(new ExtractRequest
        {
            ExtractRequestId = informatiefExtractRequestId,
            ExternalRequestId = $"INWINNING_{niscode}",
            Description = Fixture.Create<string>(),
            OrganizationCode = Fixture.Create<string>(),
            RequestedOn = DateTimeOffset.UtcNow,
            CurrentDownloadId = Fixture.Create<Guid>()
        });
        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Fixture.Create<Guid>(),
            ExtractRequestId = informatiefExtractRequestId,
            Closed = true,
            ClosedOn = DateTimeOffset.UtcNow,
            IsInformative = true,
            Contour = Polygon.Empty,
            Status = ExtractDownloadStatus.Available,
            RequestedOn = DateTimeOffset.UtcNow,
            ZipArchiveWriterVersion = Fixture.Create<string>()
        });

        var nietInformatiefExtractRequestId = Fixture.Create<string>();
        _extractsDbContext.ExtractRequests.Add(new ExtractRequest
        {
            ExtractRequestId = nietInformatiefExtractRequestId,
            ExternalRequestId = $"INWINNING_{niscode}",
            Description = Fixture.Create<string>(),
            OrganizationCode = Fixture.Create<string>(),
            RequestedOn = DateTimeOffset.UtcNow,
            CurrentDownloadId = Fixture.Create<Guid>()
        });
        var nietInformatiefDownloadId = Fixture.Create<Guid>();
        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = nietInformatiefDownloadId,
            ExtractRequestId = nietInformatiefExtractRequestId,
            Closed = false,
            IsInformative = false,
            Contour = Polygon.Empty,
            Status = ExtractDownloadStatus.Available,
            RequestedOn = DateTimeOffset.UtcNow,
            ZipArchiveWriterVersion = Fixture.Create<string>()
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
        responseObject.HistoriekExtracten.Should().HaveCount(1);
        responseObject.HistoriekExtracten.Single().DownloadId.Should().Be(new DownloadId(nietInformatiefDownloadId));
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
