namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Framework;
    using Model;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class Organizations : IOrganizations
    {
        private static readonly Func<OrganizationId, StreamName> StreamNameFactory =
            id => new StreamName(id.ToString()).WithPrefix("organization-");

        private readonly EventSourcedEntityMap _map;
        private readonly IStreamStore _store;

        public Organizations(EventSourcedEntityMap map, IStreamStore store)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<bool> Exists(OrganizationId id, CancellationToken ct = default)
        {
            var stream = StreamNameFactory(id);
            if (_map.TryGet(stream, out _))
            {
                return true;
            }
            var page = await _store.ReadStreamForwards(stream, StreamVersion.Start, 1, ct);
            return page.Status == PageReadStatus.Success;
        }
    }
}
