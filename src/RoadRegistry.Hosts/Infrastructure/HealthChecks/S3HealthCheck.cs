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
    public const string DummyFilePath = "./healthcheck.bin";

    private readonly string _objectKey;

    private readonly string _bucketName;
    private readonly S3Options _options;
    private readonly Permission _permission;
    
    public S3HealthCheck(S3Options options, string bucketName, Permission permission, string applicationName)
    {
        _options = options;
        _bucketName = bucketName;
        _permission = permission;
        _objectKey = $"healthcheck.{applicationName}.bin";
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
                            Key = _objectKey,
                            BucketName = _bucketName,
                            FilePath = DummyFilePath
                        };
                        var putObjectResponse = await client.PutObjectAsync(putObjectRequest, cancellationToken);
                        if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return HealthCheckResult.Unhealthy(description: $"Received status code {putObjectResponse.HttpStatusCode} when putting the file");
                        }

                        var getObjectResponse = await client.GetObjectAsync(_bucketName, _objectKey, cancellationToken);
                        if (getObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                        {
                            return HealthCheckResult.Unhealthy(description: $"Received status code {getObjectResponse.HttpStatusCode} when getting the file");
                        }

                        var deleteObjectResponse = await client.DeleteObjectAsync(_bucketName, _objectKey, cancellationToken);
                        if (deleteObjectResponse.HttpStatusCode != HttpStatusCode.NoContent)
                        {
                            return HealthCheckResult.Unhealthy(description: $"Received status code {deleteObjectResponse.HttpStatusCode} when deleting the file");
                        }

                        return HealthCheckResult.Healthy();
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
