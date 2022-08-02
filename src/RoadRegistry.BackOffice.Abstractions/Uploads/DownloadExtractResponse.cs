namespace RoadRegistry.BackOffice.Abstractions.Uploads;

using Microsoft.Net.Http.Headers;

public sealed record DownloadExtractResponse(string FileName, MediaTypeHeaderValue MediaTypeHeaderValue, Func<Stream, CancellationToken, Task> Callback) : FileResponse(FileName, MediaTypeHeaderValue, Callback)
{
}
