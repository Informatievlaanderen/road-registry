namespace RoadRegistry.Hosts;

using System;
using System.Data.Common;
using System.Linq;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public static class ProgramConfigurationLogExtensions
{
    public static void LogBlobClientCredentials<T>(this ILogger<T> logger, BlobClientOptions blobClientOptions)
    {
        if (blobClientOptions == null) throw new ArgumentNullException(nameof(blobClientOptions));
        
        logger.LogInformation("BlobClientType set to: {BlobClientType}", blobClientOptions.BlobClientType);

        switch (blobClientOptions.BlobClientType)
        {
            case nameof(S3BlobClient):
            {
                var s3ServiceUrl = Environment.GetEnvironmentVariable("S3__SERVICEURL");

                if (s3ServiceUrl is not null)
                {
                    logger.LogInformation("LOCALSTACK set to: {LOCALSTACK_GATEWAY}", s3ServiceUrl);
                }
                break;
            }
        }
    }

    public static void LogSqlServerConnectionString<T>(
        this ILogger<T> logger,
        IConfiguration configuration,
        string connectionName)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(connectionName);

        var connectionString = configuration.GetRequiredConnectionString(connectionName);
        
        logger.LogInformation("{ConnectionName} connection string set to:{ConnectionString}",
            connectionName,
            new DbConnectionStringBuilder
            {
                ConnectionString = connectionString,
                ["Password"] = "**REDACTED**"
            }.ConnectionString);
    }

    public static void LogKnownSqlServerConnectionStrings<T>(this ILogger<T> logger, IConfiguration configuration)
    {
        var connectionStringNames = configuration
            .GetSection("ConnectionStrings")
            .GetChildren()
            .Select(x => x.Key)
            .ToArray();

        foreach (var connectionStringName in connectionStringNames)
        {
            logger.LogSqlServerConnectionString(configuration, connectionStringName);
        }
    }
}
