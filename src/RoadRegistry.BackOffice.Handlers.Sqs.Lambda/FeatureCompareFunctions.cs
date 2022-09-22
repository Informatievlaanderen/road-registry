using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Commands;
using Core;
using Extracts;
using Hosts;
using MediatR;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;
using RoadRegistry.BackOffice.Framework;
using SqlStreamStore;
using Uploads;
using ZipArchiveWriters.Validation;

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
        var message = JsonConvert.DeserializeObject<SqsJsonMessage>(@event.Records.Single().Body, _serializerSettings).Map();

        await _serviceProvider
            .GetRequiredService<IMediator>()
            .Send(new InitiateFeatureCompareDockerContainerCommand(context)
            {
                ArchiveId = RetrieveArchiveId(message)
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
    /// <param name="message">The message.</param>
    /// <param name="context">The context.</param>
    /// <returns><see cref="ArchiveId"/> which will be used to find blob inside the bucket.</returns>
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
