namespace RoadRegistry.Legacy.Extract.Readers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;

    public class CompositeEventReader : IEventReader
    {
        public CompositeEventReader(params IEventReader[] readers)
        {
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        }

        private readonly IEventReader[] _readers;

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            foreach (var reader in _readers)
            foreach (var @event in reader.ReadEvents(connection))
                yield return @event;
        }
    }
}
