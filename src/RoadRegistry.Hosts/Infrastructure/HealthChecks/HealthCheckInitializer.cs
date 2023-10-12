namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Linq;
using Amazon.Runtime;
using BackOffice.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Namotion.Reflection;
using Options;
using TicketingService.Abstractions;

public class HealthCheckInitializer
{
    private readonly IHealthChecksBuilder _builder;
    private readonly IConfiguration _configuration;
    private readonly bool _isDevelopment;

    private HealthCheckInitializer(IHealthChecksBuilder builder, IConfiguration configuration, bool isDevelopment)
    {
        _builder = builder;
        _configuration = configuration;
        _isDevelopment = isDevelopment;
    }

    public HealthCheckInitializer AddAcmIdm()
    {
        var optionsBuilder = new AcmIdmHealthCheckOptionsBuilder();
        if (optionsBuilder.IsValid)
        {
            _builder.Add(new HealthCheckRegistration(
                "auth-acm-idm".ToLowerInvariant(),
                sp => new AcmIdmHealthCheck(optionsBuilder.Build()),
                default,
                new[] { "auth", "acm", "idm" },
                default));
        }
        return this;
    }

    public HealthCheckInitializer AddLambda(Action<LambdaHealthCheckOptionsBuilder> setup)
    {
        var optionsBuilder = new LambdaHealthCheckOptionsBuilder();
        setup?.Invoke(optionsBuilder);

        if (optionsBuilder.IsValid)
        {
            var options = optionsBuilder.Build();

            _builder.Add(new HealthCheckRegistration(
                $"lambda-healthcheck".ToLowerInvariant(),
                sp => new LambdaHealthCheck(options),
                default,
                new[] { "aws", "lamda" },
                default));
        }
        return this;
    }

    public HealthCheckInitializer AddS3(Action<S3HealthCheckOptionsBuilder> setup)
    {
        var optionsBuilder = new S3HealthCheckOptionsBuilder(_configuration);
        setup?.Invoke(optionsBuilder);

        if (optionsBuilder.IsValid)
        {
            var s3BlobClientOptions = _configuration.GetOptions<S3BlobClientOptions>()
                                      ?? new S3BlobClientOptions();
            var s3Options = _isDevelopment
                ? _configuration.GetOptions<DevelopmentS3Options>()
                : _configuration.GetOptions<S3Options>();

            foreach (var bucketPermissions in optionsBuilder.GetPermissions())
            {
                var bucketKey = bucketPermissions.Item1;
                var permissions = bucketPermissions.Item2;

                var bucketName = s3BlobClientOptions.FindBucketName(bucketKey) ?? bucketKey;

                foreach (var permission in permissions)
                {
                    _builder.Add(new HealthCheckRegistration(
                        $"s3-{bucketKey}-{permission}".ToLowerInvariant(),
                        sp => new S3HealthCheck(s3Options, bucketName, permission),
                        default,
                        new[] { "aws", "s3" },
                        default));
                }
            }
        }
        return this;
    }

    public HealthCheckInitializer AddSqlServer()
    {
        var connectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren();
        if (connectionStrings is not null)
        {
            foreach (var connectionString in connectionStrings.Where(x => x.Value is not null))
            {
                _builder.AddSqlServer(
                    connectionString.Value!,
                    name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                    tags: new[] { "db", "sql", "sqlserver" });
            }
        }

        return this;
    }

    public HealthCheckInitializer AddSqs(Action<SqsHealthCheckOptionsBuilder> setup)
    {
        var optionsBuilder = new SqsHealthCheckOptionsBuilder(_configuration);
        setup?.Invoke(optionsBuilder);

        if (optionsBuilder.IsValid)
        {
            var sqsOptions = _configuration.GetOptions<SqsConfiguration>()
                ?? new SqsConfiguration();
            var sqsQueueUrlOptions = _configuration.GetOptions<SqsQueueUrlOptions>()
                ?? new SqsQueueUrlOptions();

            foreach (var bucketPermissions in optionsBuilder.GetPermissions())
            {
                var queueName = bucketPermissions.Item1;
                var permissions = bucketPermissions.Item2;

                var healthCheckOptions = new SqsHealthCheckOptions
                {
                    //RegionEndpoint = RegionEndpoint.EUWest1,
                    ServiceUrl = sqsOptions.ServiceUrl,
                    Credentials = _isDevelopment ? new BasicAWSCredentials("dummy", "dummy") : null,
                    QueueUrl = sqsQueueUrlOptions.TryGetPropertyValue<string>(queueName) ?? queueName
                };

                foreach (var permission in permissions)
                {
                    _builder.Add(new HealthCheckRegistration(
                        $"sqs-{queueName}-{permission}".ToLowerInvariant(),
                        sp => new SqsHealthCheck(healthCheckOptions, permission),
                        default,
                        new[] { "aws", "sqs" },
                        default));
                }
            }
        }

        return this;
    }

    public HealthCheckInitializer AddTicketing()
    {
        var optionsBuilder = new TicketingHealthCheckOptionsBuilder();
        if (optionsBuilder.IsValid)
        {
            _builder.Add(new HealthCheckRegistration(
                "ticketing-service".ToLowerInvariant(),
                sp => new TicketingHealthCheck(optionsBuilder.With(sp.GetService<ITicketing>()).Build()),
                default,
                new[] { "ticketing" },
                default));
        }
        return this;
    }

    public HealthCheckInitializer AddKafka()
    {
        return this;
    }

    public static HealthCheckInitializer Configure(IHealthChecksBuilder builder, IConfiguration configuration, bool isDevelopment)
    {
        return new HealthCheckInitializer(builder, configuration, isDevelopment);
    }
}
