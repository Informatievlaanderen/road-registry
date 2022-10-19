using Ductus.FluentDocker.Services;

namespace RoadRegistry.BackOffice.Abstractions.FeatureCompare;

public record CheckFeatureCompareDockerContainerMessageResponse(ServiceRunningState State) : SqsMessageResponse
{
    public bool IsRunning => State == ServiceRunningState.Running;
}
