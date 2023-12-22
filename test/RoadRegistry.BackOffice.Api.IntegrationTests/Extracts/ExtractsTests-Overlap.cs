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
    using System.Net.Http.Json;
    using System.Threading;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using GeoJSON.Net.Feature;
    using Xunit;

    internal static class HttpClientExtensions
    {
        public static async Task<T?> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
        {
            var response = await client.PostAsJsonAsync(requestUri, value, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task<DownloadExtractResponseBody> PostExtractsDownloadrequests(this HttpClient client, DownloadExtractRequestBody request, CancellationToken cancellationToken)
        {
            var response = await client.PostAsJsonAsync("v1/extracts/downloadrequests", request, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<DownloadExtractResponseBody>(json);
        }

        public static async Task<FeatureCollection> GetOverlappingTransactionZonesGeoJson(this HttpClient client, CancellationToken cancellationToken)
        {
            return await client.GetFromJsonAsync<FeatureCollection>("v1/extracts/overlappingtransactionzones.geojson", cancellationToken);
        }
    }

    //public partial class ExtractsTests
    //{
    //    //TODO-rik unit test with overlap
    //    [Fact]
    //    public async Task WhenExtractGotRequestedWithOverlap()
    //    {
    //        var apiClient = await Fixture.CreateApiClient(new[] { Scopes.DvWrIngemetenWegBeheer });

    //        var extractRequest1 = await apiClient.PostExtractsDownloadrequests(new DownloadExtractRequestBody(
    //            "MULTIPOLYGON(((55000 200000,55000 200001,55001 200001,55001 200000,55000 200000)))",
    //            $"TEST_{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
    //            false
    //            ), CancellationToken.None);

    //        var extractRequest2 = await apiClient.PostExtractsDownloadrequests(new DownloadExtractRequestBody(
    //            "MULTIPOLYGON(((55000.5 200000,55000.5 200001,55001.5 200001,55001.5 200000,55000.5 200000)))",
    //            $"TEST_{DateTime.Today:yyyyMMdd}_{DateTime.Now:HHmmssfff}",
    //            false
    //            ), CancellationToken.None);


    //        //TODO-rik end2end test
    //        var overlappings = await apiClient.GetOverlappingTransactionZonesGeoJson(CancellationToken.None);

    //        //get overlapping geometries, both should exist in there

    //        //close both extracts
    //        //PUT {downloadId}/close
    //        //{ Reason: "" }
    //        //RoadNetworkExtractCloseReason
    //    }

    //    //TODO-rik unit test closing extract should delete overlap
    //}
}
