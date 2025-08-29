namespace RoadRegistry.BackOffice.Abstractions.Files
{
    using Microsoft.Net.Http.Headers;

    public sealed record DownloadFileResponse(string FileName, MediaTypeHeaderValue MediaTypeHeaderValue, Func<Stream, CancellationToken, Task> Callback) : FileResponse(FileName, MediaTypeHeaderValue, Callback)
    {
    }
}
