namespace RoadRegistry.Tests.BackOffice.Uploads;

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extracts.Uploads;
using Scenarios;

public class TranslatedChangesTests
{
    [Fact]
    public async Task WhenInvalidTranslatedChanges_ThenThrowZipArchiveValidationException()
    {
        var fixture = new RoadNetworkTestData().ObjectProvider;
        var translatedChanges = TranslatedChanges.Empty;

        var sut = () => translatedChanges.ToChangeRoadNetworkCommand(
            NullLogger.Instance,
            fixture.Create<ExtractRequestId>(),
            fixture.Create<ChangeRequestId>(),
            fixture.Create<DownloadId>(),
            null,
            CancellationToken.None);
        await sut.Should().ThrowAsync<ZipArchiveValidationException>();
    }
}
