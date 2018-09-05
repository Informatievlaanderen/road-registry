namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.AggregateSource;
    using Aiv.Vbr.EventHandling;
    using Framework;
    using Model;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class RoadNetworks : IRoadNetworks
    {
        public static readonly StreamName Stream = new StreamName("roadnetwork");

        private readonly ConcurrentUnitOfWork _unitOfWork;
        private readonly IStreamStore _store;
        private readonly JsonSerializerSettings _settings;
        private readonly EventMapping _mapping;

        public RoadNetworks(ConcurrentUnitOfWork unitOfWork, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        public async Task<RoadNetwork> Get(CancellationToken ct = default)
        {
            if (_unitOfWork.TryGet(Stream, out Aggregate aggregate))
            {
                return (RoadNetwork)aggregate.Root;
            }
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
