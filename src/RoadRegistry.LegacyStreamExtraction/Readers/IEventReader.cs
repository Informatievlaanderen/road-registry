namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using BackOffice.Framework;

    public interface IEventReader
    {
        Task<IReadOnlyCollection<RecordedEvent>> ReadAsync(SqlConnection connection);
    }
}
