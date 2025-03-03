namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public abstract class RoadRegistryCommandQueue
{
    protected static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    private readonly IStreamStore _store;
    private readonly EventMapping _commandMapping;
    private readonly ApplicationMetadata _applicationMetadata;

    protected RoadRegistryCommandQueue(IStreamStore store, EventMapping commandMapping, ApplicationMetadata applicationMetadata = null)
    {
        _store = store.ThrowIfNull();
        _commandMapping = commandMapping.ThrowIfNull();
        _applicationMetadata = applicationMetadata;
    }

    protected async Task AppendToStoreStream(Command command, StreamName streamName, CancellationToken cancellationToken)
    {
        var jsonMetadata = JsonConvert.SerializeObject(
            new MessageMetadata
            {
                Processor = _applicationMetadata?.MessageProcessor ?? RoadRegistryApplication.BackOffice,
                ProvenanceData = command.ProvenanceData
            },
            SerializerSettings);
        await _store.AppendToStream(streamName, ExpectedVersion.Any, [
            new NewStreamMessage(
                command.MessageId,
                _commandMapping.GetEventName(command.Body.GetType()),
                JsonConvert.SerializeObject(command.Body, SerializerSettings),
                jsonMetadata)
        ], cancellationToken);
    }
}
