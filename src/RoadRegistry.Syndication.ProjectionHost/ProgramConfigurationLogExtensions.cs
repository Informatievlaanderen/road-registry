namespace RoadRegistry.Syndication.ProjectionHost
{
    using System;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    internal static class ProgramConfigurationLogExtensions
    {
        public static void LogSqlServerConnectionString(
            this ILogger<Program> logger,
            IConfiguration configuration,
            string connectionName)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (connectionName == null) throw new ArgumentNullException(nameof(connectionName));

            logger.LogInformation("{ConnectionName} connection string set to:{ConnectionString}",
                connectionName,
                new SqlConnectionStringBuilder(configuration.GetConnectionString(connectionName))
                {
                    Password = "**REDACTED**"
                }.ConnectionString);
        }
    }
}
