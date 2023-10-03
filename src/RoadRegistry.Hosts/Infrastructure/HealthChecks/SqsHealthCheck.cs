namespace RoadRegistry.Hosts.Infrastructure.HealthChecks;

using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using BackOffice;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Options;

internal class SqsHealthCheck : IHealthCheck
{
    private readonly Permission _permission;
    private readonly SqsHealthCheckOptions _sqsOptions;

    public SqsHealthCheck(SqsHealthCheckOptions sqsOptions, Permission permission)
    {
        _sqsOptions = sqsOptions ?? throw new ArgumentNullException(nameof(SqsHealthCheckOptions));
        _permission = permission;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var serializer = JsonSerializer.CreateDefault(SqsJsonSerializerSettingsProvider.CreateSerializerSettings());
            var client = CreateSqsClient();

            using (client)
            {
                switch (_permission)
                {
                    case Permission.Read:
                        var receiveMessageResponse = await client.ReceiveMessageAsync(_sqsOptions.QueueUrl, cancellationToken);
                        break;
                    case Permission.Write:
                        var sqsJsonMessage = SqsJsonMessage.Create(new HealthCheckSqsMessage(), serializer);
                        var messageBody = serializer.Serialize(sqsJsonMessage);
                        var sendMessageRequest = new SendMessageRequest
                        {
                            QueueUrl = _sqsOptions.QueueUrl,
                            MessageBody = messageBody,
                            MessageGroupId = "HealthCheck"
                        };
                        var sendMessageResponse = await client.SendMessageAsync(sendMessageRequest, cancellationToken);
                        break;
                    case Permission.Delete:
                        break;
                    default:
                        return HealthCheckResult.Degraded();
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private IAmazonSQS CreateSqsClient()
    {
        var credentialsProvided = _sqsOptions.Credentials is not null;
        var regionProvided = _sqsOptions.RegionEndpoint is not null;
        var serviceUrlProvided = _sqsOptions.ServiceUrl is not null;

        return (credentialsProvided, regionProvided, serviceUrlProvided) switch
        {
            (false, false, false) => new AmazonSQSClient(),
            (false, true, false) => new AmazonSQSClient(_sqsOptions.RegionEndpoint),
            (true, false, false) => new AmazonSQSClient(_sqsOptions.Credentials),
            (true, true, false) => new AmazonSQSClient(_sqsOptions.Credentials, _sqsOptions.RegionEndpoint),
            (false, false, true) => new AmazonSQSClient(new AmazonSQSConfig { ServiceURL = _sqsOptions.ServiceUrl }),
            (false, true, true) => new AmazonSQSClient(new AmazonSQSConfig { ServiceURL = _sqsOptions.ServiceUrl, RegionEndpoint = _sqsOptions.RegionEndpoint }),
            (true, false, true) => new AmazonSQSClient(_sqsOptions.Credentials, new AmazonSQSConfig { ServiceURL = _sqsOptions.ServiceUrl }),
            (true, true, true) => new AmazonSQSClient(_sqsOptions.Credentials, new AmazonSQSConfig { ServiceURL = _sqsOptions.ServiceUrl, RegionEndpoint = _sqsOptions.RegionEndpoint })
        };
    }
}

internal class HealthCheckSqsMessage : SQSEvent.SQSMessage
{
    public string MessageGroupId => "HealthCheck";
}
