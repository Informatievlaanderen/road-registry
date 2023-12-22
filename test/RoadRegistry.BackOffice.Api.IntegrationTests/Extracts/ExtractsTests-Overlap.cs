using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts
{
    using System.Net.Http;
    using Newtonsoft.Json;
    using RoadRegistry.BackOffice.Api.Extracts;
    using RoadRegistry.StreetName;
    using System.Net.Http.Json;
    using System.Threading;
    using Xunit;

    internal static class HttpClientExtensions
    {
        public static async Task<T?> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
        {
            var response = await client.PostAsJsonAsync($"v1/extracts/downloadrequests", value, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    internal partial class ExtractsTests
    {
        [Fact]
        public async Task WhenExtractGotRequestedWithOverlap()
        {
            var extractRequest1 = await ApiClient.PostAsJsonAsync<DownloadExtractResponseBody>("v1/extracts/downloadrequests", new DownloadExtractRequestBody(
                "MULTIPOLYGON(((55000 200000,55000 200001,55001 200001,55001 200000,55000 200000)))",
                $"TEST_{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
                false
                ), CancellationToken.None);

            var extractRequest2 = await ApiClient.PostAsJsonAsync<DownloadExtractResponseBody>("v1/extracts/downloadrequests", new DownloadExtractRequestBody(
                "MULTIPOLYGON(((55000.5 200000,55000.5 200001,55001.5 200001,55001.5 200000,55000.5 200000)))",
                $"TEST_{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
                false
                ), CancellationToken.None);

            
            //TODO-rik end2end test
            //request new extract 1
            //POST downloadrequests
            // { string Contour, string RequestId, bool IsInformative }
            // response: (string DownloadId, bool IsInformative)

            //request new extract 2 with overlapping geometries

            //get overlapping geometries, both should exist in there

            //close both extracts
            //PUT {downloadId}/close
            //{ Reason: "" }
            //RoadNetworkExtractCloseReason
        }
    }
}
