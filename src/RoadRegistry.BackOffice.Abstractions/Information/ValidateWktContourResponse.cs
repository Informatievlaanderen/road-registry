namespace RoadRegistry.BackOffice.Abstractions.Information;

public sealed record ValidateWktContourResponse : EndpointResponse
{
    public double Area { get; set; }
    public int AreaMaximumSquareKilometers = 100;
    public bool IsValid { get; set; }
    public bool IsLargerThanMaximumArea => Area > (AreaMaximumSquareKilometers * 1000 * 1000);
}
