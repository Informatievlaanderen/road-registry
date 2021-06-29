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
        public Task when_requesting_an_extract_again()
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
    }
}
