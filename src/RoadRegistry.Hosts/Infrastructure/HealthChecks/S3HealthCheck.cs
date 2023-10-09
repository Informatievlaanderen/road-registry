namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using BackOffice;
using BackOffice.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal class S3HealthCheck : IHealthCheck
{
    private const string ObjectKey = "healthcheck.dummy.bin";
    private readonly string _bucketName;
    private readonly S3Options _options;
    private readonly Permission _permission;

    public S3HealthCheck(S3Options options, string bucketName, Permission permission)
    {
        _options = options;
        _bucketName = bucketName;
        _permission = permission;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _options.CreateS3Client();

            using (client)
            {
                switch (_permission)
                {
                    case Permission.Read:
                        var listObjectsResponse = await client.ListObjectsAsync(_bucketName, cancellationToken);
                        return listObjectsResponse is not null && listObjectsResponse.HttpStatusCode == HttpStatusCode.OK
                            ? HealthCheckResult.Healthy()
                            : HealthCheckResult.Unhealthy();

                    case Permission.Write:
                        var putObjectRequest = new PutObjectRequest
                        {
                            Key = ObjectKey,
                            BucketName = _bucketName,
                            FilePath = "./paket.references"
                        };
                        var putObjectResponse = await client.PutObjectAsync(putObjectRequest, cancellationToken);
                        var getObjectResponse = await client.GetObjectAsync(_bucketName, ObjectKey, cancellationToken);
                        var deleteObjectResponse = await client.DeleteObjectAsync(_bucketName, ObjectKey, cancellationToken);

                        return putObjectResponse.HttpStatusCode == HttpStatusCode.OK &&
                               getObjectResponse.HttpStatusCode == HttpStatusCode.OK &&
                               deleteObjectResponse.HttpStatusCode == HttpStatusCode.NoContent
                            ? HealthCheckResult.Healthy()
                            : HealthCheckResult.Unhealthy();
                    default:
                        return HealthCheckResult.Degraded();
                }
            }
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
