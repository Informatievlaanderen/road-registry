namespace RoadRegistry
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.EventHandling;
    using Framework;
    using Model;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class RoadNetworks : IRoadNetworks
    {
        public static readonly StreamName Stream = new StreamName("roadnetwork");

        private readonly IStreamStore _store;
        private readonly JsonSerializerSettings _settings;
        private readonly EventMapping _mapping;

        public RoadNetworks(IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        public async Task<RoadNetwork> Get(CancellationToken ct = default)
        {
            var page = await _store.ReadStreamForwards(Stream, StreamVersion.Start, 1024, ct);
            if (page.Status == PageReadStatus.StreamNotFound) return RoadNetwork.Factory();
            IEventSource root = RoadNetwork.Factory();
            var messages = new List<object>(page.Messages.Length);
            foreach (var message in page.Messages)
            {
                messages.Add(
                    JsonConvert.DeserializeObject(
                        await message.GetJsonData(ct),
                        _mapping.GetEventType(message.Type),
                        _settings));
            }
            root.RestoreFromEvents(messages.ToArray());
            while (!page.IsEnd)
            {
                messages.Clear();
                page = await page.ReadNext(ct);
                if (page.Status == PageReadStatus.StreamNotFound) return RoadNetwork.Factory();
                foreach (var message in page.Messages)
                {
                    messages.Add(
                        JsonConvert.DeserializeObject(
                            await message.GetJsonData(ct),
                            _mapping.GetEventType(message.Type),
                            _settings));
                }
                root.RestoreFromEvents(messages.ToArray());
            }
            return (RoadNetwork)root;
        }
    }
}
