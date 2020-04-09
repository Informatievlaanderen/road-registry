namespace RoadRegistry.BackOffice.EventHost
{
    using System;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Configuration;
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

        public static void LogBlobClientCredentials(this ILogger<Program> logger, BlobClientOptions blobClientOptions)
        {
            if (blobClientOptions == null) throw new ArgumentNullException(nameof(blobClientOptions));

            const int revealCharacterCount = 4;

            logger.LogInformation("BlobClientType set to: {BlobClientType}", blobClientOptions.BlobClientType);

            switch (blobClientOptions.BlobClientType)
            {
                case nameof(S3BlobClient):
                {
                    // Use MINIO
                    if (Environment.GetEnvironmentVariable("MINIO_SERVER") != null)
                    {
                        logger.LogInformation("MINIO_SERVER set to: {MINIO_SERVER}",
                            Environment.GetEnvironmentVariable("MINIO_SERVER"));
                        logger.LogInformation("MINIO_ACCESS_KEY set to: {MINIO_ACCESS_KEY}",
                            Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") ?? "<null>");
                        var minioSecretKey = Environment.GetEnvironmentVariable("MINIO_SECRET_KEY");
                        if (minioSecretKey != null && minioSecretKey.Length >= revealCharacterCount)
                        {
                            logger.LogInformation(
                                "MINIO_SECRET_KEY set to: {MINIO_SECRET_KEY_START}...{MINIO_SECRET_KEY_END}",
                                minioSecretKey.Substring(0, revealCharacterCount),
                                minioSecretKey.Substring(minioSecretKey.Length - revealCharacterCount,
                                    revealCharacterCount));
                        }
                    }
                    else
                    {
                        logger.LogInformation("AWS_ACCESS_KEY_ID set to: {AWS_ACCESS_KEY_ID}",
                            Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "<null>");
                        var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
                        if (awsSecretAccessKey != null && awsSecretAccessKey.Length >= revealCharacterCount)
                        {
                            logger.LogInformation(
                                "AWS_SECRET_ACCESS_KEY set to: {AWS_SECRET_ACCESS_KEY_START}...{AWS_SECRET_ACCESS_KEY_END}",
                                awsSecretAccessKey.Substring(0, revealCharacterCount),
                                awsSecretAccessKey.Substring(awsSecretAccessKey.Length - revealCharacterCount,
                                    revealCharacterCount));
                        }
                    }

                    break;
                }
            }
        }
    }
}
