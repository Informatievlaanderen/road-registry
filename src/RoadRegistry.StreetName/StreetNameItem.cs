namespace RoadRegistry.StreetName;

public record StreetNameItem
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string Status { get; init; }
    public string NisCode { get; init; }
}
