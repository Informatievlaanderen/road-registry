//namespace RoadRegistry.BackOffice.Api.Infrastructure.SystemHealthChecks;

//using System;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using Amazon.S3;
//using Amazon.S3.Model;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
//using RoadRegistry.BackOffice;

//internal class S3HealthCheck : ISystemHealthCheck
//{
//    public const string DummyFilePath = "./healthcheck.bin";

//    private readonly string _bucketName;
//    private readonly IAmazonS3 _s3Client;
//    private readonly Permission _permission;

//    public S3HealthCheck(IAmazonS3 s3Client, Permission permission)
//    {
//        _s3Client = s3Client;
//        _bucketName = bucketName;
//        _permission = permission;
//    }

//    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            switch (_permission)
//            {
//                case Permission.Read:
//                    var listObjectsResponse = await _s3Client.ListObjectsAsync(_bucketName, cancellationToken);
//                    return listObjectsResponse is not null && listObjectsResponse.HttpStatusCode == HttpStatusCode.OK
//                        ? HealthCheckResult.Healthy()
//                        : HealthCheckResult.Unhealthy();

//                case Permission.Write:
//                    var objectKey = $"healthcheck.{_applicationName}.{DateTime.Now:yyyyMMdd-HHmmssfff}.bin";
//                    var putObjectRequest = new PutObjectRequest
//                    {
//                        Key = objectKey,
//                        BucketName = _bucketName,
//                        FilePath = DummyFilePath
//                    };
//                    var putObjectResponse = await _s3Client.PutObjectAsync(putObjectRequest, cancellationToken);
//                    if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
//                    {
//                        return HealthCheckResult.Unhealthy(description: $"Received status code {putObjectResponse.HttpStatusCode} when putting the file");
//                    }

//                    DeleteObjectResponse deleteObjectResponse;
//                    try
//                    {
//                        var getObjectResponse = await _s3Client.GetObjectAsync(_bucketName, objectKey, cancellationToken);
//                        if (getObjectResponse.HttpStatusCode != HttpStatusCode.OK)
//                        {
//                            return HealthCheckResult.Unhealthy(description: $"Received status code {getObjectResponse.HttpStatusCode} when getting the file");
//                        }
//                    }
//                    finally
//                    {
//                        deleteObjectResponse = await _s3Client.DeleteObjectAsync(_bucketName, objectKey, cancellationToken);
//                    }

//                    if (deleteObjectResponse.HttpStatusCode != HttpStatusCode.NoContent)
//                    {
//                        return HealthCheckResult.Unhealthy(description: $"Received status code {deleteObjectResponse.HttpStatusCode} when deleting the file");
//                    }

//                    return HealthCheckResult.Healthy();
//                default:
//                    return HealthCheckResult.Degraded();
//            }
//        }
//        catch (Exception ex)
//        {
//            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
//        }
//    }
//}
