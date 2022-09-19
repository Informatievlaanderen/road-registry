namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using Abstractions;
    using Amazon.Lambda.Core;
    using Ductus.FluentDocker.Extensions;
    using Ductus.FluentDocker.Services;
    using RoadRegistry.BackOffice.Abstractions.Exceptions;
    using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Commands;

    public class InitiateFeatureCompareDockerContainerCommandHandler : LambdaCommandHandler<InitiateFeatureCompareDockerContainerCommand>
    {
        public override Task HandleAsync(InitiateFeatureCompareDockerContainerCommand command, ILambdaContext context, CancellationToken cancellationToken)
        {
            var container = FeatureCompareDockerContainerBuilder.Default
                .WithEnvironment(GetEnvironmentVariableCollection())
                .Build();

            using (container.Start())
            {
                var config = container.GetConfiguration(true);
                var running = ServiceRunningState.Running == config.State.ToServiceState();

                if (!running)
                {
                    context.Logger.LogCritical($"Feature compare docker container exited on startup! Input: {command.ArchiveId.ToString()}");
                    throw new FeatureCompareDockerContainerNotRunningException("Feature compare docker container exited on startup!", command.ArchiveId);
                }
            }

            context.Logger.LogInformation($"Feature compare container started for {command.ArchiveId.ToString()}");
            return Task.CompletedTask;

            string[] GetEnvironmentVariableCollection()
            {
                Dictionary<string, string> collection = new()
                {
                    { "BLOBNAME", command.ArchiveId },
                    { "MINIO_SERVER", Environment.GetEnvironmentVariable("MINIO_SERVER") },
                    { "MINIO_ROOT_USER", Environment.GetEnvironmentVariable("MINIO_ROOT_USER") },
                    { "MINIO_ROOT_PASSWORD", Environment.GetEnvironmentVariable("MINIO_ROOT_PASSWORD") },
                    { "AWS_ACCESS_KEY_ID", Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") },
                    { "AWS_SECRET_ACCESS_KEY", Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") }
                };
                return collection.Select(kvp => $"{kvp.Key}={kvp.Value}").ToArray();
            }
        }


    }
}
