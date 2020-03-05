namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;

    public class CompositeEventReader : IEventReader
    {
        private readonly IEventReader[] _readers;

        public CompositeEventReader(params IEventReader[] readers)
        {
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        }

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            foreach (var reader in _readers)
            {
                foreach (var @event in reader.ReadEvents(connection))
                {
                    yield return @event;
                }
            }
        }
    }
}
