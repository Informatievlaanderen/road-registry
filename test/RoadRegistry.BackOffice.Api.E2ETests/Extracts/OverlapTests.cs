namespace RoadRegistry.BackOffice.Api.E2ETests.Extracts
{
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using E2ETests;
    using Messages;
    using RoadRegistry.BackOffice.Api.Extracts;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    public class OverlapTests : IClassFixture<ApiClientTestFixture>
    {
        private const string ExtractDescriptionPrefix = "E2ETest_";

        protected ApiClientTestFixture Fixture { get; }
        protected ITestOutputHelper OutputHelper { get; }

        public OverlapTests(ApiClientTestFixture fixture, ITestOutputHelper outputHelper)
        {
            Fixture = fixture;
            OutputHelper = outputHelper;
        }

        [Fact]
        public async Task WhenExtractGotRequestedWithOverlap()
        {
            var apiClient = await Fixture.CreateBackOfficeApiClient(new[] { Scopes.DvWrIngemetenWegBeheer });
            if (apiClient is null)
            {
                return;
            }

            await CloseRemainingTestExtracts(apiClient);

            var extractRequest1 = await apiClient.RequestDownloadExtract(new DownloadExtractRequestBody(
                "MULTIPOLYGON(((55000 200000,55000 200100,55100 200100,55100 200000,55000 200000)))",
                $"{ExtractDescriptionPrefix}{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
                false
                ), CancellationToken.None);

            var extractRequest2 = await apiClient.RequestDownloadExtract(new DownloadExtractRequestBody(
                "MULTIPOLYGON(((55050 200000,55050 200100,55150 200100,55150 200000,55050 200000)))",
                $"{ExtractDescriptionPrefix}{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
                false
                ), CancellationToken.None);

            OutputHelper.WriteLine("Requested extract 1: {0}", extractRequest1.DownloadId);
            OutputHelper.WriteLine("Requested extract 2: {0}", extractRequest2.DownloadId);

            // Wait until overlap is created
            try
            {
                var sw = Stopwatch.StartNew();
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    var overlappings = await apiClient.GetOverlappingTransactionZonesGeoJson(CancellationToken.None);

                    var matchingOverlap = overlappings.Features
                        .SingleOrDefault(x => Equals(x.Properties["downloadId1"], extractRequest1.DownloadId) && Equals(x.Properties["downloadId2"], extractRequest2.DownloadId));
                    if (matchingOverlap is not null)
                    {
                        OutputHelper.WriteLine("Overlap found");
                        break;
                    }

                    if (sw.Elapsed.TotalMinutes > 2)
                    {
                        Assert.Fail($"Timed out, waited {sw.Elapsed} for overlap to be created");
                    }
                }
            }
            finally
            {
                await Task.WhenAll(
                    apiClient.CloseExtract(extractRequest1.DownloadId, new CloseRequestBody { Reason = RoadNetworkExtractCloseReason.InformativeExtract.ToString() }, CancellationToken.None),
                    apiClient.CloseExtract(extractRequest2.DownloadId, new CloseRequestBody { Reason = RoadNetworkExtractCloseReason.InformativeExtract.ToString() }, CancellationToken.None)
                );
            }

            // Assert that overlap is removed
            {
                var sw = Stopwatch.StartNew();
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));

                    var overlappings = await apiClient.GetOverlappingTransactionZonesGeoJson(CancellationToken.None);

                    var matchingOverlap = overlappings.Features
                        .SingleOrDefault(x => Equals(x.Properties["downloadId1"], extractRequest1.DownloadId) && Equals(x.Properties["downloadId2"], extractRequest2.DownloadId));
                    if (matchingOverlap is null)
                    {
                        OutputHelper.WriteLine("Overlap is removed");
                        break;
                    }

                    if (sw.Elapsed.TotalMinutes > 2)
                    {
                        Assert.Fail($"Timed out, waited {sw.Elapsed} for overlap to be removed");
                    }
                }
            }
        }

        private async Task CloseRemainingTestExtracts(BackOfficeApiHttpClient apiClient)
        {
            var overlappings = await apiClient.GetOverlappingTransactionZonesGeoJson(CancellationToken.None);

            var downloadIds = Array.Empty<string>()
                .Concat(overlappings.Features
                    .Where(x => ((string)x.Properties["description1"]).StartsWith(ExtractDescriptionPrefix))
                    .Select(x => (string)x.Properties["downloadId1"]))
                .Concat(overlappings.Features
                    .Where(x => ((string)x.Properties["description2"]).StartsWith(ExtractDescriptionPrefix))
                    .Select(x => (string)x.Properties["downloadId2"]))
                .Distinct()
                .ToList();

            if (downloadIds.Any())
            {
                await Task.WhenAll(downloadIds.Select(downloadId => apiClient.CloseExtract(downloadId, new CloseRequestBody { Reason = RoadNetworkExtractCloseReason.InformativeExtract.ToString() }, CancellationToken.None)));
            }
        }
    }
}
