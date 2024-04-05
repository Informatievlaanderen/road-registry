namespace RoadRegistry.BackOffice.Api.E2ETests;

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Api.Extracts;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

internal static class BackOfficeApiHttpClientExtensions
{
    public static async Task<DownloadExtractResponseBody> RequestDownloadExtract(this BackOfficeApiHttpClient client, DownloadExtractRequestBody request, CancellationToken cancellationToken)
    {
        return await client.PostAsJsonAsync<DownloadExtractResponseBody>("v1/extracts/downloadrequests", request, cancellationToken);
    }

    public static async Task<Stream> DownloadExtract(this BackOfficeApiHttpClient client, string downloadId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var response = await client.GetAsync($"v1/extracts/download/{downloadId}", cancellationToken);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    continue;
                case HttpStatusCode.OK:
                    return await response.Content.ReadAsStreamAsync(cancellationToken);
            }

            throw new Exception($"Unexpected response {response.StatusCode} when trying to download the extract {downloadId}");
        }
    }

    public static async Task UploadExtract(this BackOfficeApiHttpClient client, byte[] archiveBytes, CancellationToken cancellationToken)
    {
        while (true)
        {
            var content = new MultipartFormDataContent
            {
                Headers =
                {
                    //ContentType = MediaTypeHeaderValue.Parse("multipart/form-data")
                    ContentType = MediaTypeHeaderValue.Parse("application/zip")
                }
            };
            //content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            //{
            //    Name = "archive",
            //    FileName = "archive.zip"
            //};
            content.Add(new ByteArrayContent(archiveBytes), "archive", "archive.zip");
            
            var response = await client.PostAsync("v1/upload/fc", content, cancellationToken);

            var contentString = await response.Content.ReadAsStringAsync(cancellationToken);

            //return await response.Content.ReadAsStreamAsync(cancellationToken);
            return;
        }
    }

    public static async Task CloseExtract(this BackOfficeApiHttpClient client, string downloadId, CloseRequestBody request, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync($"v1/extracts/{downloadId}/close", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<FeatureCollection> GetOverlappingTransactionZonesGeoJson(this BackOfficeApiHttpClient client, CancellationToken cancellationToken)
    {
        var json = await client.GetStringAsync("v1/extracts/overlappingtransactionzones.geojson", cancellationToken);
        return JsonConvert.DeserializeObject<FeatureCollection>(json);
    }
}
