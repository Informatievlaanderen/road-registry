namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using Messages;

    public class MemoryRoadNetworkSnapshotReaderWriter : IRoadNetworkSnapshotReader, IRoadNetworkSnapshotWriter
    {
        private readonly IRoadNetworkSnapshotReader _fallbackReader;
        private CachedRoadNetworkSnapshot _cache;

        // NOTE: For reading we fallback to another snapshot reader
        // For writing we don't since we don't want to pay serialization and I/O tax nor do we want to deal with possible race conditions (depending on how the roadnetwork command handling is hosted)
        // Writing new snapshots is still done asynchronously, by a single thread in a single process.

        public MemoryRoadNetworkSnapshotReaderWriter(IRoadNetworkSnapshotReader fallbackReader)
        {
            _fallbackReader = fallbackReader ?? throw new ArgumentNullException(nameof(fallbackReader));
            _cache = null;
        }
        public async Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
        {
            var cached = Interlocked.CompareExchange(ref _cache, null, null);
            if (cached != null)
            {
                return (cached.Snapshot, cached.Version);
            }

            var (snapshot, version) = await _fallbackReader.ReadSnapshot(cancellationToken);
            if (snapshot != null)
            {
                Interlocked.CompareExchange(
                    ref _cache,
                    new CachedRoadNetworkSnapshot { Snapshot = snapshot, Version = version },
                    null);
            }
            return (snapshot, version);
        }

        public Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
        {
            Interlocked.Exchange(ref _cache,
                new CachedRoadNetworkSnapshot { Snapshot = snapshot, Version = version });
            return Task.CompletedTask;
        }

        private class CachedRoadNetworkSnapshot
        {
            public RoadNetworkSnapshot Snapshot { get; set; }
            public int Version { get; set; }
        }
    }

    public class Memory2RoadNetworkSnapshotReaderWriter : IRoadNetworkSnapshotReader, IRoadNetworkSnapshotWriter
    {
        private readonly IRoadNetworkSnapshotReader _fallbackReader;
        private readonly ConcurrentDictionary<int, CachedRoadNetworkSnapshot> _cache;

        // NOTE: For reading we fallback to another snapshot reader
        // For writing we don't since we don't want to pay serialization and I/O tax nor do we want to deal with possible race conditions (depending on how the roadnetwork command handling is hosted)
        // Writing new snapshots is still done asynchronously, by a single thread in a single process.

        public Memory2RoadNetworkSnapshotReaderWriter(IRoadNetworkSnapshotReader fallbackReader)
        {
            _fallbackReader = fallbackReader ?? throw new ArgumentNullException(nameof(fallbackReader));
            _cache = new ConcurrentDictionary<int, CachedRoadNetworkSnapshot>();
        }
        public async Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
        {
            _cache.T
            if (_cache.TryPeek(out var cached))
            {
                return (cached.Snapshot, cached.Version);
            }

            var (snapshot, version) = await _fallbackReader.ReadSnapshot(cancellationToken);
            if (snapshot != null)
            {
                _cache.Add(new CachedRoadNetworkSnapshot { Snapshot = snapshot, Version = version });
                foreach (var item in _cache.ToArray())
                {
                    _cache.R
                }
                Interlocked.CompareExchange(
                    ref _cache,
                    new CachedRoadNetworkSnapshot { Snapshot = snapshot, Version = version },
                    null);
            }
            return (snapshot, version);
        }

        public Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
        {
            Interlocked.Exchange(ref _cache,
                new CachedRoadNetworkSnapshot { Snapshot = snapshot, Version = version });
            return Task.CompletedTask;
        }

        private class CachedRoadNetworkSnapshot
        {
            public RoadNetworkSnapshot Snapshot { get; set; }
            public int Version { get; set; }
        }
    }
}
