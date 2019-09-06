namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using BackOffice.Framework;

    public class CompositeEventReader : IEventReader
    {
        private readonly IEventReader[] _readers;

        public CompositeEventReader(params IEventReader[] readers)
        {
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        }

        public async Task<IReadOnlyCollection<RecordedEvent>> ReadAsync(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var events = ImmutableList<RecordedEvent>.Empty;
            foreach (var reader in _readers)
            {
                events = events.AddRange(await reader.ReadAsync(connection));
            }

            return events;
        }
    }
}
