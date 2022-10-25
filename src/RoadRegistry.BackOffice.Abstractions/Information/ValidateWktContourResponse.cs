namespace RoadRegistry.BackOffice.Abstractions.Information;

using FluentValidation;

public sealed record ValidateWktContourResponse(string Contour) : EndpointResponse
{
    public ValidationException Exception { get; init; }
}