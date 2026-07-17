namespace RoadRegistry.BackOffice;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IDbMigratorFactory
{
    IDbMigrator CreateMigrator(IConfiguration configuration, ILoggerFactory loggerFactory);
}
