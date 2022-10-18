namespace RoadRegistry.BackOffice.Abstractions.Downloads;

using Microsoft.Net.Http.Headers;

public sealed record DownloadProductResponse(string FileName, MediaTypeHeaderValue MediaTypeHeaderValue, Func<Stream, CancellationToken, Task> Callback) : FileResponse(FileName, MediaTypeHeaderValue, Callback)
{
}