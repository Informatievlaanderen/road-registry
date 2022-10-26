namespace RoadRegistry.BackOffice.Abstractions.Information;

public sealed record ValidateWktContourRequest(string Contour) : EndpointRequest<ValidateWktContourResponse>
{
}