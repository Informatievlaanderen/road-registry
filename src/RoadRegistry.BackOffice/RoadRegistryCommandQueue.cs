namespace RoadRegistry.BackOffice;

using System;
using System.Linq;
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
    private readonly CommandMetadata _commandMetadata;

    protected RoadRegistryCommandQueue(IStreamStore store, EventMapping commandMapping, CommandMetadata commandMetadata = null)
    {
        _store = store.ThrowIfNull();
        _commandMapping = commandMapping;
        _commandMetadata = commandMetadata;
    }

    protected async Task AppendToStoreStream(Command command, StreamName streamName, CancellationToken cancellationToken)
    {
        var jsonMetadata = JsonConvert.SerializeObject(
            new MessageMetadata
            {
                Principal = command
                    .Principal
                    .Claims
                    .Select(claim => new Claim { Type = claim.Type, Value = claim.Value })
                    .ToArray(),
                Processor = _commandMetadata?.Processor ?? RoadRegistryApplication.BackOffice
            },
            SerializerSettings);
        await _store.AppendToStream(streamName, ExpectedVersion.Any, new[]
        {
            new NewStreamMessage(
                command.MessageId,
                _commandMapping.GetEventName(command.Body.GetType()),
                JsonConvert.SerializeObject(command.Body, SerializerSettings),
                jsonMetadata)
        }, cancellationToken);
    }
}
