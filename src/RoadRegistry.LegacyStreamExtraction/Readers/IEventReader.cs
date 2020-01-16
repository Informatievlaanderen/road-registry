namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface IEventReader
    {
        IEnumerable<StreamEvent> ReadEvents(SqlConnection connection);
    }
}
