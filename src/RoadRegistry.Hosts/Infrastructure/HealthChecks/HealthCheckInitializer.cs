namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
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
            var s3BlobClientOptions = _configuration.GetOptions<S3BlobClientOptions>();
            if (s3BlobClientOptions?.Buckets is not null)
            {
                var s3Options = _isDevelopment
                    ? _configuration.GetOptions<DevelopmentS3Options>()
                    : _configuration.GetOptions<S3Options>();

                foreach (var bucketName in s3BlobClientOptions.Buckets)
                {
                    var permissions = optionsBuilder.GetPermissions(bucketName.Key);
                    foreach (var permission in permissions)
                    {
                        _builder.Add(new HealthCheckRegistration(
                            $"s3-{bucketName.Key}-{permission.ToString()}".ToLowerInvariant(),
                            sp => new S3HealthCheck(s3Options, bucketName.Value, permission),
                            default,
                            new[] { "aws", "s3" },
                            default));
                    }
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
            foreach (var connectionString in connectionStrings)
            {
                _builder.AddSqlServer(
                    connectionString.Value,
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
            var sqsOptions = _configuration.GetOptions<SqsConfiguration>();
            var sqsQueueUrlOptions = _configuration.GetOptions<SqsQueueUrlOptions>();

            if (sqsQueueUrlOptions is not null)
            {
                var sqsQueueUrlOptionsType = sqsQueueUrlOptions.GetType();
                var sqsQueueUrlOptionsProps = sqsQueueUrlOptionsType.GetProperties();

                foreach (var propertyInfo in sqsQueueUrlOptionsProps)
                {
                    var healthCheckOptions = new SqsHealthCheckOptions
                    {
                        //RegionEndpoint = RegionEndpoint.EUWest1,
                        ServiceUrl = sqsOptions?.ServiceUrl,
                        Credentials = _isDevelopment ? new BasicAWSCredentials("dummy", "dummy") : null,
                        QueueUrl = sqsQueueUrlOptions.TryGetPropertyValue<string>(propertyInfo.Name)
                    };

                    var permissions = optionsBuilder.GetPermissions(propertyInfo.Name);
                    foreach (var permission in permissions)
                    {
                        _builder.Add(new HealthCheckRegistration(
                            $"sqs-{propertyInfo.Name}-{permission.ToString()}".ToLowerInvariant(),
                            sp => new SqsHealthCheck(healthCheckOptions, permission),
                            default,
                            new[] { "aws", "sqs" },
                            default));
                    }
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
