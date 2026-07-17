namespace RoadRegistry.BackOffice;

using System.Threading;
using System.Threading.Tasks;

public interface IDbMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken);
}
