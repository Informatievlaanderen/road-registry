namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record CloseExtractRequest(string ExtractRequestId) : EndpointRequest<CloseExtractResponse>
{
}
