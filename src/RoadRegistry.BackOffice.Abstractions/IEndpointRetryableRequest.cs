namespace RoadRegistry.BackOffice.Abstractions;

public interface IEndpointRetryableRequest
{
    public int DefaultRetryAfter { get; init; }
    public int RetryAfterAverageWindowInDays { get; init; }
}
