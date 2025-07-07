namespace RoadRegistry.BackOffice.Api.IntegrationTests.Extracts;

using System;
using System.IO;
using System.IO.Compression;
using Api.Extracts;
using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extracts;
using Abstractions.Jobs;
using RoadSegments;

internal static class HttpClientExtensions
{
    public static async Task<DownloadExtractResponseBody> RequestDownloadExtract(this HttpClient client, DownloadExtractRequestBody request, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync<DownloadExtractResponseBody>("v1/extracts/downloadrequests", request, cancellationToken);
        return response!;
    }

    public static async Task<ZipArchive> DownloadExtract(this HttpClient client, string downloadId, MemoryStream zipArchiveStream, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync($"v1/extracts/download/{downloadId}", cancellationToken);
        await using var readStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await readStream.CopyToAsync(zipArchiveStream, cancellationToken);
        zipArchiveStream.Position = 0;

        return new ZipArchive(zipArchiveStream, ZipArchiveMode.Update, leaveOpen: true);
    }

    public static async Task<ZipArchive> DownloadExtractAndWait(this HttpClient client, string downloadId, MemoryStream zipArchiveStream, CancellationToken cancellationToken)
    {
        var downloadStopWatch = System.Diagnostics.Stopwatch.StartNew();
        while (true)
        {
            if (downloadStopWatch.Elapsed.TotalMinutes > 5)
            {
                throw new TimeoutException("Download of extract took too long.");
            }

            try
            {
                return await DownloadExtract(client, downloadId, zipArchiveStream, cancellationToken);
            }
            catch
            {
                Thread.Sleep(3000);
            }
        }
    }

    public static async Task CloseExtract(this HttpClient client, string downloadId, CloseRequestBody request, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync($"v1/extracts/{downloadId}/close", request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public static async Task Upload(this HttpClient client, MemoryStream zipArchiveStream, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync<GetPresignedUploadUrlResponse>($"v1/upload/jobs", null, cancellationToken);

        using var form = new MultipartFormDataContent();

        zipArchiveStream.Position = 0;
        var fileContent = new StreamContent(zipArchiveStream);
        //fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        form.Add(
            content: fileContent,
            name: "file",
            fileName: "archive.zip"
        );

        foreach (var formData in response.UploadUrlFormData)
        {
            form.Add(new StringContent(formData.Value), formData.Key);

            //form.Add(new StringContent(formData.Value), $"\"{formData.Key}\"");

            // var stringContent1 = new StringContent(formData.Value);
            // //stringContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = $"\"{formData.Key}\"" };
            // stringContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = $"{formData.Key}" };
            // form.Add(stringContent1);
        }

        var formAsString = await form.ReadAsStringAsync();
        using var awsClient = new HttpClient();
        var uploadResponse = await awsClient.PostAsync(response.UploadUrl, form);
        var responseBody = await uploadResponse.Content.ReadAsStringAsync();

        uploadResponse.EnsureSuccessStatusCode();
    }

    public static async Task<ExtractDetailsResponseBody> GetExtractDetails(this HttpClient client, string downloadId, CancellationToken cancellationToken)
    {
        var response = await client.GetFromJsonAsync<ExtractDetailsResponseBody>($"v1/extracts/{downloadId}", cancellationToken);
        return response!;
    }

    public static async Task<GetRoadSegmentResponse> GetRoadSegment(this HttpClient client, int roadSegmentId, CancellationToken cancellationToken)
    {
        var response = await client.GetFromJsonAsync<GetRoadSegmentResponse>($"v1/wegsegmenten/{roadSegmentId}", cancellationToken);
        return response!;
    }

    public static async Task<FeatureCollection> GetOverlappingTransactionZonesGeoJson(this HttpClient client, CancellationToken cancellationToken)
    {
        var json = await client.GetStringAsync("v1/extracts/overlappingtransactionzones.geojson", cancellationToken);
        return JsonConvert.DeserializeObject<FeatureCollection>(json)!;
    }

    private static async Task<T?> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
    {
        var response = await client.PostAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }

    private static async Task<T?> PutAsJsonAsync<T>(this HttpClient client, string requestUri, object value, CancellationToken cancellationToken)
    {
        var response = await client.PutAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
