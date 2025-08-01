namespace RoadRegistry.BackOffice.Abstractions.Extracts.V2;

public sealed record RequestExtractRequest(string ExtractRequestId, string Contour, string Description, bool IsInformative) : EndpointRequest<RequestExtractResponse>
{
}
