namespace RoadRegistry.BackOffice.Contracts;

using Microsoft.Net.Http.Headers;

public abstract record FileResponse(string FileName, MediaTypeHeaderValue MediaTypeHeaderValue, Func<Stream, CancellationToken, Task> Callback) : EndpointResponse
{
}
