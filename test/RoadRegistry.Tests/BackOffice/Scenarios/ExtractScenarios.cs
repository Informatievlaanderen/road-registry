namespace RoadRegistry.BackOffice.Scenarios
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Xunit;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Extracts;
    using NodaTime.Text;
    using RoadRegistry.Framework.Testing;
    using Uploads;

    public class ExtractScenarios: RoadRegistryFixture
    {
        public ExtractScenarios()
        {
            Fixture.CustomizeExternalExtractRequestId();
            Fixture.CustomizeRoadNetworkExtractGeometry();
            Fixture.CustomizeArchiveId();
        }

        [Fact]
        public Task when_requesting_an_extract_for_the_first_time()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            return Run(scenario => scenario
                .GivenNone()
                .When(TheExternalSystem.PutsInARoadNetworkExtractRequest(externalExtractRequestId, downloadId, contour))
                .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_requesting_an_extract_again_with_a_different_download_id()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var oldDownloadId = Fixture.Create<DownloadId>();
            var newDownloadId = Fixture.Create<DownloadId>();
            var oldContour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();
            var newContour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            return Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = oldDownloadId,
                    Contour = oldContour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.PutsInARoadNetworkExtractRequest(externalExtractRequestId, newDownloadId, newContour))
                .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = newDownloadId,
                    Contour = newContour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_requesting_an_extract_again_with_the_same_download_id()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var oldContour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();
            var newContour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            return Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = oldContour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.PutsInARoadNetworkExtractRequest(externalExtractRequestId, downloadId, newContour))
                .ThenNone()
            );
        }

        [Fact]
        public Task when_announcing_a_requested_road_network_extract_download_became_available()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            return Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(OurSystem.AnnouncesRoadNetworkExtractDownloadBecameAvailable(extractRequestId, downloadId, archiveId))
                .Then(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
            );
        }

        [Fact]
        public Task when_announcing_an_announced_road_network_extract_download_became_available()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            return Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(OurSystem.AnnouncesRoadNetworkExtractDownloadBecameAvailable(extractRequestId, downloadId, archiveId))
                .ThenNone()
            );
        }

        [Fact]
        public async Task when_uploading_an_archive_of_changes_for_an_outdated_download()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var outdatedDownloadId = Fixture.Create<DownloadId>();
            var downloadId = Fixture.Create<DownloadId>();
            var uploadId = Fixture.Create<UploadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            await CreateEmptyArchive(archiveId);

            await Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = outdatedDownloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, outdatedDownloadId, uploadId, archiveId))
                .Throws(new CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload(externalExtractRequestId, extractRequestId, outdatedDownloadId, downloadId, uploadId))
            );
        }

        private async Task CreateEmptyArchive(ArchiveId archiveId)
        {
            using (var stream = new MemoryStream())
            {
                using (new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
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

        [Fact]
        public async Task when_uploading_an_archive_of_changes_for_an_unknown_download()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var unknownDownloadId = Fixture.Create<DownloadId>();
            var downloadId = Fixture.Create<DownloadId>();
            var uploadId = Fixture.Create<UploadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            await CreateEmptyArchive(archiveId);

            await Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, unknownDownloadId, uploadId, archiveId))
                .Throws(new CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownload(externalExtractRequestId, extractRequestId, unknownDownloadId, uploadId))
            );
        }

        [Fact]
        public async Task when_uploading_an_archive_of_changes_which_are_accepted_after_validation()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var uploadId = Fixture.Create<UploadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            await CreateEmptyArchive(archiveId);

            await Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, downloadId, uploadId, archiveId))
                .Then(RoadNetworkExtracts.ToStreamName(extractRequestId),
                    new Messages.RoadNetworkExtractChangesArchiveUploaded
                    {
                        RequestId = extractRequestId,
                        ExternalRequestId = externalExtractRequestId,
                        DownloadId = downloadId,
                        UploadId = uploadId,
                        ArchiveId = archiveId,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    },
                    new Messages.RoadNetworkExtractChangesArchiveAccepted
                    {
                        RequestId = extractRequestId,
                        ExternalRequestId = externalExtractRequestId,
                        DownloadId = downloadId,
                        UploadId = uploadId,
                        ArchiveId = archiveId,
                        Problems = Array.Empty<Messages.FileProblem>(),
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    })
            );
        }

        [Fact]
        public async Task when_uploading_an_archive_of_changes_which_are_not_accepted_after_validation()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var uploadId = Fixture.Create<UploadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            await CreateErrorArchive(archiveId);

            await Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, downloadId, uploadId, archiveId))
                .Then(RoadNetworkExtracts.ToStreamName(extractRequestId),
                    new Messages.RoadNetworkExtractChangesArchiveUploaded
                    {
                        RequestId = extractRequestId,
                        ExternalRequestId = externalExtractRequestId,
                        DownloadId = downloadId,
                        UploadId = uploadId,
                        ArchiveId = archiveId,
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    },
                    new Messages.RoadNetworkExtractChangesArchiveRejected
                    {
                        RequestId = extractRequestId,
                        ExternalRequestId = externalExtractRequestId,
                        DownloadId = downloadId,
                        UploadId = uploadId,
                        ArchiveId = archiveId,
                        Problems = new []
                        {
                            new Messages.FileProblem
                            {
                                File = "error",
                                Severity = Messages.ProblemSeverity.Error,
                                Reason = "reason",
                                Parameters = Array.Empty<Messages.ProblemParameter>()
                            }
                        },
                        When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                    })
            );
        }

        [Fact]
        public async Task when_uploading_an_archive_of_changes_a_second_time()
        {
            var externalExtractRequestId = Fixture.Create<ExternalExtractRequestId>();
            var extractRequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId);
            var downloadId = Fixture.Create<DownloadId>();
            var uploadId = Fixture.Create<UploadId>();
            var archiveId = Fixture.Create<ArchiveId>();
            var contour = Fixture.Create<Messages.RoadNetworkExtractGeometry>();

            await CreateErrorArchive(archiveId);

            await Run(scenario => scenario
                .Given(RoadNetworkExtracts.ToStreamName(extractRequestId), new Messages.RoadNetworkExtractGotRequested
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    Contour = contour,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                }, new Messages.RoadNetworkExtractDownloadBecameAvailable
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new Messages.RoadNetworkExtractChangesArchiveUploaded
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                },
                new Messages.RoadNetworkExtractChangesArchiveAccepted
                {
                    RequestId = extractRequestId,
                    ExternalRequestId = externalExtractRequestId,
                    DownloadId = downloadId,
                    UploadId = uploadId,
                    ArchiveId = archiveId,
                    Problems = Array.Empty<Messages.FileProblem>(),
                    When = InstantPattern.ExtendedIso.Format(Clock.GetCurrentInstant())
                })
                .When(TheExternalSystem.UploadsRoadNetworkExtractChangesArchive(extractRequestId, downloadId, uploadId, archiveId))
                .Throws(new CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce(externalExtractRequestId, extractRequestId, downloadId, uploadId))
            );
        }
    }
}
