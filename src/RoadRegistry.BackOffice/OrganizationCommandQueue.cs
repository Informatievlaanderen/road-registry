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

public class OrganizationCommandQueue : IOrganizationCommandQueue
{
    private static readonly EventMapping CommandMapping =
        new(OrganizationCommands.All.ToDictionary(command => command.Name));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
    
    private readonly IStreamStore _store;

    public OrganizationCommandQueue(IStreamStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task Write(OrganizationId organizationId, Command command, CancellationToken cancellationToken)
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
        await _store.AppendToStream(OrganizationId.ToStreamName(organizationId), ExpectedVersion.Any, new[]
        {
            new NewStreamMessage(
                command.MessageId,
                CommandMapping.GetEventName(command.Body.GetType()),
                JsonConvert.SerializeObject(command.Body, SerializerSettings),
                jsonMetadata)
        }, cancellationToken);
    }

    private sealed class Claim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    private sealed class CommandMetadata
    {
        public Claim[] Principal { get; set; }
    }
}
