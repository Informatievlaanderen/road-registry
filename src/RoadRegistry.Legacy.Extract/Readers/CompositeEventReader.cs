namespace RoadRegistry.Legacy.Extract.Readers;

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
        ArgumentNullException.ThrowIfNull(connection);

        foreach (var reader in _readers)
        {
            foreach (var @event in reader.ReadEvents(connection))
            {
                yield return @event;
            }
        }
    }
}
