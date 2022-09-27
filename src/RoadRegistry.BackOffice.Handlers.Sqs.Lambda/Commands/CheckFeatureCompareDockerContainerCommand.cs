namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Commands
{
    using Abstractions;
    using Amazon.Lambda.Core;

    public record CheckFeatureCompareDockerContainerCommand(ILambdaContext Context) : LambdaCommand<CheckFeatureCompareDockerContainerCommandResponse>(Context)
    {
    }
}
