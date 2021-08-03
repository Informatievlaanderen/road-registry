namespace RoadRegistry.BackOffice.Scenarios
{
    using System.Threading.Tasks;
    using Xunit;
    using AutoFixture;
    using Extracts;
    using NodaTime.Text;
    using RoadRegistry.Framework.Testing;

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

        //when_uploading_an_archive_of_changes_for_an_outdated_download
        //when_uploading_an_archive_of_changes_for_an_unknown_download
        //when_uploading_an_archive_of_changes
        //when_the_uploaded_archive_of_changes_can_not_be_accepted_after_validation
        //when_the_uploaded_archive_of_changes_can_be_accepted_after_validation
    }
}
