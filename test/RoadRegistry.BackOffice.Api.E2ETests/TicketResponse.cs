namespace RoadRegistry.BackOffice.Api.E2ETests;

using Newtonsoft.Json;

internal sealed record TicketResponse(string Status, TicketResponse.TicketResult Result)
{
    public T GetResult<T>()
    {
        return JsonConvert.DeserializeObject<T>(Result.Json);
    }

    internal sealed record TicketResult(string Json);
}
