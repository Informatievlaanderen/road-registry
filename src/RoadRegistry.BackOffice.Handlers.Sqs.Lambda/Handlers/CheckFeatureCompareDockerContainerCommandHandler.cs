namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using Abstractions;
    using Amazon.Lambda.Core;
    using Commands;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Extensions;

    public class CheckFeatureCompareDockerContainerCommandHandler : LambdaCommandHandler<CheckFeatureCompareDockerContainerCommand, CheckFeatureCompareDockerContainerCommandResponse>
    {
        public override Task<CheckFeatureCompareDockerContainerCommandResponse> HandleAsync(CheckFeatureCompareDockerContainerCommand request, ILambdaContext context, CancellationToken cancellationToken)
        {
            var container = FeatureCompareDockerContainerBuilder.Default.Build();

            CheckFeatureCompareDockerContainerCommandResponse response;
            using (container)
            {
                var serviceState = container.GetConfiguration(true).State.ToServiceState();
                response = new CheckFeatureCompareDockerContainerCommandResponse(serviceState);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                context.Logger.LogInformation("Received cancellation request. Exit without failure. See you on the next timer run!");
                return Task.FromCanceled<CheckFeatureCompareDockerContainerCommandResponse>(cancellationToken);
            }

            if (response.IsRunning)
            {
                context.Logger.LogInformation("Feature compare container found. Exit without failure. See you on the next timer run!");
                return Task.FromCanceled<CheckFeatureCompareDockerContainerCommandResponse>(cancellationToken);
            }

            return Task.FromResult<CheckFeatureCompareDockerContainerCommandResponse>(response);
        }
    }
}
