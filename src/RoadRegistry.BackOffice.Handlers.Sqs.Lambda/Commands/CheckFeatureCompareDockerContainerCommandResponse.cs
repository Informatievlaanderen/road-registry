namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Commands;

using Abstractions;
using Ductus.FluentDocker.Services;

public record CheckFeatureCompareDockerContainerCommandResponse(ServiceRunningState State) : LambdaCommandResponse
{
    public bool IsRunning => State == ServiceRunningState.Running;
}
