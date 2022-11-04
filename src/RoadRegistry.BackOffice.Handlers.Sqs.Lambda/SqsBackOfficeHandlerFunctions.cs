using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Commands;
using MediatR;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

public class SqsBackOfficeHandlerFunctions
{
    private readonly JsonSerializerSettings _serializerSettings;
    private readonly IServiceProvider _serviceProvider;

    public SqsBackOfficeHandlerFunctions()
    {
        var services = new ServiceCollection();
        var builder = new ContainerBuilder();
        builder.RegisterModule(new MediatorModule());
        builder.Populate(services);
        _serviceProvider = new AutofacServiceProvider(builder.Build());
        _serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    }

    /// <summary>
    ///     Checks the feature compare docker container.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    public async Task CheckFeatureCompareDockerContainer(ILambdaContext context, CancellationToken cancellationToken)
    {
        try
        {
            await _serviceProvider
                .GetRequiredService<IMediator>()
                .Send(new CheckFeatureCompareDockerContainerCommand(context), cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // intentionally left blank
        }
    }

    /// <summary>
    ///     Initiates the feature compare docker container.
    /// </summary>
    /// <param name="event">The event.</param>
    /// <param name="context">The context.</param>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    public async Task InitiateFeatureCompareDockerContainer(SQSEvent @event, ILambdaContext context, CancellationToken cancellationToken)
    {
        var message = JsonConvert.DeserializeObject<SqsJsonMessage>(@event.Records.Single().Body, _serializerSettings).Map();

        await _serviceProvider
            .GetRequiredService<IMediator>()
            .Send(new InitiateFeatureCompareDockerContainerCommand(context)
            {
                ArchiveId = RetrieveArchiveId(message)
            }, cancellationToken);
    }

    /// <summary>
    ///     Retrieves the archive identifier.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="context">The context.</param>
    /// <returns><see cref="ArchiveId" /> which will be used to find blob inside the bucket.</returns>
    private static ArchiveId RetrieveArchiveId(object message)
    {
        var archiveId = message switch
        {
            UploadRoadNetworkChangesArchive uploadRoadNetworkChangesArchive => new ArchiveId(uploadRoadNetworkChangesArchive.ArchiveId),
            UploadRoadNetworkExtractChangesArchive uploadRoadNetworkExtractChangesArchive => new ArchiveId(uploadRoadNetworkExtractChangesArchive.ArchiveId),
            _ => throw new NotImplementedException($"Could not find an archive ID from type {message.GetType().FullName}")
        };

        return archiveId;
    }
}
