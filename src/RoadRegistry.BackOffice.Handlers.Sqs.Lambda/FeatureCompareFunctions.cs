using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Commands;
using MediatR;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Framework;

public class FeatureCompareFunctions
{
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IServiceProvider _serviceProvider;

    public FeatureCompareFunctions()
    {
        var services = new ServiceCollection();
        var builder = new ContainerBuilder();
        builder.RegisterModule(new MediatorModule());
        builder.Populate(services);
        _serviceProvider = new AutofacServiceProvider(builder.Build());
        _serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    }

    /// <summary>
    /// Initiates the feature compare docker container.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public async Task InitiateFeatureCompareDockerContainer(SQSEvent @event, ILambdaContext context, CancellationToken cancellationToken)
    {
        var message = @event.Records.Single();
        var queueCommand = JsonConvert.DeserializeObject<SimpleQueueCommand>(message.Body, _serializerSettings);

        await _serviceProvider
            .GetRequiredService<IMediator>()
            .Send(new InitiateFeatureCompareDockerContainerCommand(context)
            {
                ArchiveId = RetrieveArchiveId(queueCommand, context)
            }, cancellationToken);
    }

    /// <summary>
    /// Checks the feature compare docker container.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public async Task CheckFeatureCompareDockerContainer(ILambdaContext context, CancellationToken cancellationToken)
    {
        await _serviceProvider
            .GetRequiredService<IMediator>()
            .Send(new CheckFeatureCompareDockerContainerCommand(context), cancellationToken);
    }

    /// <summary>
    /// Retrieves the archive identifier.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="context">The context.</param>
    /// <returns><see cref="ArchiveId"/> which will be used to find blob inside the bucket.</returns>
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
