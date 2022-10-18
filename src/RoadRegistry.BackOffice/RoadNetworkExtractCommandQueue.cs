namespace RoadRegistry.BackOffice;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using Newtonsoft.Json;
using SqlStreamStore;
using SqlStreamStore.Streams;

public class RoadNetworkExtractCommandQueue : IRoadNetworkExtractCommandQueue
{
    private readonly IStreamStore _store;

    public RoadNetworkExtractCommandQueue(IStreamStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    private sealed class Claim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    private static readonly EventMapping CommandMapping =
        new(RoadNetworkCommands.All.ToDictionary(command => command.Name));

    private sealed class CommandMetadata
    {
        public Claim[] Principal { get; set; }
    }

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public static readonly StreamName Stream = new("roadnetworkextract-command-queue");

    public async Task Write(Command command, CancellationToken cancellationToken)
    {
        var jsonMetadata = JsonConvert.SerializeObject(
            new CommandMetadata
            {
                Principal = command
                    .Principal
                    .Claims
                    .Select(claim => new Claim { Type = claim.Type, Value = claim.Value })
                    .ToArray()
            },
            SerializerSettings);
        await _store.AppendToStream(Stream, ExpectedVersion.Any, new[]
        {
            new NewStreamMessage(
                command.MessageId,
                CommandMapping.GetEventName(command.Body.GetType()),
                JsonConvert.SerializeObject(command.Body, SerializerSettings),
                jsonMetadata)
        }, cancellationToken);
    }
}