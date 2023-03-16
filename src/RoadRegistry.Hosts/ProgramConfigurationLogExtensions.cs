namespace RoadRegistry.Hosts;

using System;
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
