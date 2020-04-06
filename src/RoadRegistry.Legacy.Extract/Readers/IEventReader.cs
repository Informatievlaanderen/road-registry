namespace RoadRegistry.Legacy.Extract.Readers
{
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;

    public interface IEventReader
    {
        IEnumerable<StreamEvent> ReadEvents(SqlConnection connection);
    }
}
