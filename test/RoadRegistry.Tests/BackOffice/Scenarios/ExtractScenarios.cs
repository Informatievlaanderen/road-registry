namespace RoadRegistry.Tests.BackOffice.Scenarios;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Framework.Projections;
using Framework.Testing;
using KellermanSoftware.CompareNetObjects;
using NodaTime.Text;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Messages;
using System.IO.Compression;
using Moq;
using TicketingService.Abstractions;
using FileProblem = RoadRegistry.BackOffice.Messages.FileProblem;

public class ExtractScenarios : RoadRegistryTestBase
{
    public ExtractScenarios(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper, CreateComparisonConfig())
    {
        ObjectProvider.CustomizeExternalExtractRequestId();
        ObjectProvider.CustomizeRoadNetworkExtractGeometry();
        ObjectProvider.CustomizeExtractDescription();
        ObjectProvider.CustomizeArchiveId();
    }

    private static ComparisonConfig CreateComparisonConfig()
    {
        var comparisonConfig = new ComparisonConfig
        {
            MaxDifferences = int.MaxValue,
            MaxStructDepth = 5,
            IgnoreCollectionOrder = true
        };

        comparisonConfig.CustomPropertyComparer<RoadNetworkExtractGotRequested>(
            x => x.Contour.Polygon,
            new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()));
        comparisonConfig.CustomPropertyComparer<RoadNetworkExtractGotRequested>(
            x => x.Contour.MultiPolygon,
            new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()));

        return comparisonConfig;
    }

    private async Task CreateEmptyArchive(ArchiveId archiveId)
    {
        using (var stream = new MemoryStream())
        {
            using (new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                // what's the problem?
            }

            stream.Position = 0;
            await Client.CreateBlobAsync(
                new BlobName(archiveId),
                Metadata.None,
                ContentType.Parse("application/zip"),
                stream);
        }
    }

    private async Task CreateErrorArchive(ArchiveId archiveId)
    {
        using (var stream = new MemoryStream())
        {
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                archive.CreateEntry("error");
            }

            stream.Position = 0;
            await Client.CreateBlobAsync(
                new BlobName(archiveId),
                Metadata.None,
                ContentType.Parse("application/zip"),
                stream);
        }
    }

    private static RoadNetworkExtractGeometry FlattenContour(RoadNetworkExtractGeometry contour)
    {
        return GeometryTranslator.TranslateToRoadNetworkExtractGeometry(GeometryTranslator.Translate(contour));
    }

    [Fact]
    public Task when_announcing_a_requested_road_network_extract_download_became_available()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        return Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = contour,
                Description = extractDescription,
                IsInformative = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(OurSystem.AnnouncesRoadNetworkExtractDownloadBecameAvailable(extractRequestId, downloadId, archiveId))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractDownloadBecameAvailable
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                Description = extractDescription,
                IsInformative = true,
                OverlapsWithDownloadIds = [],
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_announcing_a_requested_road_network_extract_download_became_available_with_overlapping_downloadids()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();
        var overlapsWithDownloadIds = ObjectProvider.CreateMany<Guid>(3).ToList();

        return Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = contour,
                Description = extractDescription,
                IsInformative = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(OurSystem.AnnouncesRoadNetworkExtractDownloadBecameAvailable(extractRequestId, downloadId, archiveId, overlapsWithDownloadIds))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractDownloadBecameAvailable
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                Description = extractDescription,
                IsInformative = true,
                OverlapsWithDownloadIds = overlapsWithDownloadIds,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_announcing_a_requested_road_network_extract_download_timeout_occurred()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        return Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = contour,
                Description = extractDescription,
                IsInformative = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(OurSystem.AnnouncesRoadNetworkExtractDownloadTimeoutOccurred(extractRequestId, downloadId))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractDownloadTimeoutOccurred
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Description = extractDescription,
                IsInformative = true,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_announcing_an_announced_road_network_extract_download_became_available()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        return Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                Description = extractDescription,
                DownloadId = downloadId,
                Contour = contour,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }, new RoadNetworkExtractDownloadBecameAvailable
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                Description = extractDescription,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(OurSystem.AnnouncesRoadNetworkExtractDownloadBecameAvailable(extractRequestId, downloadId, archiveId))
            .ThenNone()
        );
    }

    [Fact]
    public Task when_requesting_an_extract_again_with_a_different_download_id()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var oldDownloadId = ObjectProvider.Create<DownloadId>();
        var newDownloadId = ObjectProvider.Create<DownloadId>();
        var oldContour = ObjectProvider.Create<RoadNetworkExtractGeometry>();
        var newContour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        return Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                Description = extractDescription,
                DownloadId = oldDownloadId,
                Contour = oldContour,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheExternalSystem.PutsInARoadNetworkExtractRequest(externalExtractRequestId, newDownloadId, extractDescription, newContour))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                Description = extractDescription,
                DownloadId = newDownloadId,
                Contour = FlattenContour(newContour),
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public Task when_requesting_an_extract_again_with_the_same_download_id()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var oldContour = ObjectProvider.Create<RoadNetworkExtractGeometry>();
        var newContour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        return Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                Description = extractDescription,
                DownloadId = downloadId,
                Contour = oldContour,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheExternalSystem.PutsInARoadNetworkExtractRequest(externalExtractRequestId, downloadId, extractDescription, newContour))
            .ThenNone()
        );
    }

    [Fact]
    public Task when_requesting_an_extract_for_the_first_time()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        return Run(scenario => scenario
            .GivenNone()
            .When(TheExternalSystem.PutsInARoadNetworkExtractRequest(externalExtractRequestId, downloadId, extractDescription, contour))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Description = extractDescription,
                Contour = FlattenContour(contour),
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
        );
    }

    [Fact]
    public async Task when_uploading_an_archive_of_changes_a_second_time()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var uploadId = ObjectProvider.Create<UploadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        await CreateErrorArchive(archiveId);

        await Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Description = extractDescription,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Description = extractDescription,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new RoadNetworkExtractChangesArchiveUploaded
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new RoadNetworkExtractChangesArchiveAccepted
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    Problems = Array.Empty<FileProblem>(),
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, downloadId, uploadId, archiveId))
            .Throws(new CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException())
        );
    }

    [Fact]
    public async Task when_uploading_an_archive_of_changes_for_an_outdated_download()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var outdatedDownloadId = ObjectProvider.Create<DownloadId>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var uploadId = ObjectProvider.Create<UploadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        await CreateEmptyArchive(archiveId);

        await Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = outdatedDownloadId,
                    Description = extractDescription,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    Description = extractDescription,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new RoadNetworkExtractGotRequestedV2
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    Description = extractDescription,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, outdatedDownloadId, uploadId, archiveId))
            .Throws(new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException(externalExtractRequestId, extractRequestId, outdatedDownloadId, downloadId, uploadId))
        );
    }

    [Fact]
    public async Task when_uploading_an_archive_of_changes_for_an_unknown_download()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var unknownDownloadId = ObjectProvider.Create<DownloadId>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var uploadId = ObjectProvider.Create<UploadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();

        await CreateEmptyArchive(archiveId);

        await Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Description = extractDescription,
                Contour = contour,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }, new RoadNetworkExtractDownloadBecameAvailable
            {
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                Description = extractDescription,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, unknownDownloadId, uploadId, archiveId))
            .Throws(new CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(externalExtractRequestId, extractRequestId, unknownDownloadId, uploadId))
        );
    }

    [Fact]
    public async Task when_uploading_an_archive_of_changes_which_are_accepted_after_validation()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var uploadId = ObjectProvider.Create<UploadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();
        var ticketId = ObjectProvider.Create<TicketId>();

        await CreateEmptyArchive(archiveId);

        await Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                Description = extractDescription,
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = contour,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }, new RoadNetworkExtractDownloadBecameAvailable
            {
                Description = extractDescription,
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, downloadId, uploadId, archiveId, ticketId))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId),
                new RoadNetworkExtractChangesArchiveUploaded
                {
                    Description = extractDescription,
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new RoadNetworkExtractChangesArchiveAccepted
                {
                    Description = extractDescription,
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    Problems = Array.Empty<FileProblem>(),
                    TicketId = ticketId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
        );

        TicketingMock
            .Verify(x => x.Error(ticketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()), Times.Never);
        TicketingMock
            .Verify(x => x.Complete(ticketId, It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task when_uploading_an_archive_of_changes_which_are_not_accepted_after_validation()
    {
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
        var extractDescription = ObjectProvider.Create<ExtractDescription>();
        var downloadId = ObjectProvider.Create<DownloadId>();
        var uploadId = ObjectProvider.Create<UploadId>();
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var contour = ObjectProvider.Create<RoadNetworkExtractGeometry>();
        var ticketId = ObjectProvider.Create<TicketId>();

        await CreateErrorArchive(archiveId);

        await Run(scenario => scenario
            .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new RoadNetworkExtractGotRequestedV2
            {
                Description = extractDescription,
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                Contour = contour,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            }, new RoadNetworkExtractDownloadBecameAvailable
            {
                Description = extractDescription,
                RequestId = extractRequestId,
                ExternalRequestId = externalExtractRequestId,
                DownloadId = downloadId,
                ArchiveId = archiveId,
                When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
            })
            .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, downloadId, uploadId, archiveId, ticketId))
            .Then(RoadNetworkExtracts.ToStreamName(extractRequestId),
                new RoadNetworkExtractChangesArchiveUploaded
                {
                    Description = extractDescription,
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new RoadNetworkExtractChangesArchiveRejected
                {
                    Description = extractDescription,
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    TicketId = ticketId,
                    Problems = new[]
                    {
                        new FileProblem
                        {
                            File = "error",
                            Severity = ProblemSeverity.Error,
                            Reason = "reason",
                            Parameters = Array.Empty<ProblemParameter>()
                        }
                    },
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
        );

        TicketingMock
            .Verify(x => x.Error(ticketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()));
    }
}
