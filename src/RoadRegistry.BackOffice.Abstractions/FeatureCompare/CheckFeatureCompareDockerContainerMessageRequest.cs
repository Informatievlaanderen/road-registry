namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

public record CheckFeatureCompareDockerContainerMessageRequest() : SqsMessageRequest<CheckFeatureCompareDockerContainerMessageResponse>
{
}
