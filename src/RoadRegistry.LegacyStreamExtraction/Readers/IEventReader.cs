namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using BackOffice.Framework;

    public interface IEventReader
    {
        IEnumerable<RecordedEvent> ReadEvents(SqlConnection connection);
    }
}
