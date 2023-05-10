namespace RoadRegistry.BackOffice.Abstractions.Information;

public sealed record ValidateWktContourResponse : EndpointResponse
{
    public double Area { get; set; }
    public int AreaMaximumSquareKilometers { get; set; }
    public bool IsValid { get; set; }
    public bool IsLargerThanMaximumArea { get; set; }
}
