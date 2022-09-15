using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Services;
using Messages;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Framework;

public class Functions
{
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly ContainerBuilder _containerBuilder;

    public Functions()
    {
        _serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        _containerBuilder = new Builder()
            .UseContainer()
            .UseImage("postgres:9.6-alpine")
            //.IsWindowsImage()
            .WithName("road-registry-feature-compare")
            .ExposePort(10000)
            .WaitForPort("10000/tcp", 30000);
    }

    /// <summary>
    /// Initiates the docker feature compare.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context.</param>
    public Task InitiateDockerFeatureCompare(SQSEvent @event, ILambdaContext context, CancellationToken cancellationToken)
    {
        var message = @event.Records.Single();
        var command = JsonConvert.DeserializeObject<SimpleQueueCommand>(message.Body, _serializerSettings);

        var archiveId = RetrieveArchiveId(command, context);

        var container = _containerBuilder
            .WithEnvironment($"INPUTBLOBNAME={archiveId}", $"OUTPUTBLOBNAME={archiveId}-results")
            .Build();

        using (container.Start())
        {
            var config = container.GetConfiguration(true);
            var running = ServiceRunningState.Running == config.State.ToServiceState();

            if (!running)
            {
                context.Logger.LogCritical($"Feature compare docker container exited on startup! Input: {archiveId.ToString()}");
                throw new FeatureCompareDockerContainerNotRunningException("Feature compare docker container exited on startup!", archiveId);
            }
        }

        context.Logger.LogInformation($"Feature compare container started for {archiveId.ToString()}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks the SQS message available for processing.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>Task.</returns>
    public Task CheckSQSMessageAvailableForProcessing(ILambdaContext context, CancellationToken cancellationToken)
    {
        var isRunning = true;
        var container = _containerBuilder.Build();

        using (container)
        {
            var serviceState = container.GetConfiguration(true).State.ToServiceState();
            isRunning = serviceState == ServiceRunningState.Running;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            context.Logger.LogInformation("Received cancellation request. Exit without failure. See you on the next timer run!");
            return Task.FromCanceled(cancellationToken);
        }

        if (isRunning)
        {
            context.Logger.LogInformation("Feature compare container found. Exit without failure. See you on the next timer run!");
            return Task.FromCanceled(cancellationToken);
        }

        // Found no active containers so we can process next message


        return Task.CompletedTask;
    }

    /// <summary>
    /// Process message as an asynchronous operation.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="context">The context.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private ArchiveId RetrieveArchiveId(SimpleQueueCommand command, ILambdaContext context)
    {
        var actualMessage = command.ToActualType(_serializerSettings);

        var archiveId = actualMessage switch
        {
            UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive => new ArchiveId(uploadRoadNetworkChangesArchive.ArchiveId),
            UploadRoadNetworkExtractChangesArchive uploadRoadNetworkExtractChangesArchive => new ArchiveId(uploadRoadNetworkExtractChangesArchive.ArchiveId),
            _ => throw new NotImplementedException($"Could not find an archive ID from type {command.Type}")
        };

        return archiveId;
    }
}
