namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Microsoft.Extensions.DependencyInjection;

public static class AmazonSqsExtensions
{
    public static async Task CreateMissingQueuesAsync(this AmazonSQSClient amazonSqsClient, Dictionary<string, Dictionary<string, string>> queueUrlAttributes, CancellationToken cancellationToken)
    {
        var queues = await amazonSqsClient.ListQueuesAsync(new ListQueuesRequest(), cancellationToken);
        
        var existingQueueUrls = queues.QueueUrls.ToArray();
        var missingQueueUrls = queueUrlAttributes
            .Select(x => x.Key)
            .Where(x => !existingQueueUrls.Contains(x))
            .ToArray();

        foreach (var queueUrl in missingQueueUrls)
        {
            try
            {
                var queueName = SqsQueue.ParseQueueNameFromQueueUrl(queueUrl);
                await amazonSqsClient.CreateQueueAsync(new CreateQueueRequest
                {
                    QueueName = queueName,
                    Attributes = queueUrlAttributes[queueUrl],
                }, cancellationToken);
            }
            catch (AmazonSQSException)
            {
                // ignore if queue already was created by a different host
            }
        }
    }

    public static async Task CreateMissingQueuesAsync(this IServiceProvider sp, CancellationToken cancellationToken)
    {
        var sqsQueueUrlOptions = sp.GetService<SqsQueueUrlOptions>();
        var queueUrlAttributes = new Dictionary<string, Dictionary<string, string>>();

        if (!string.IsNullOrEmpty(sqsQueueUrlOptions?.BackOffice))
        {
            queueUrlAttributes.Add(sqsQueueUrlOptions.BackOffice, new Dictionary<string, string>
            {
                {"FifoQueue","true"},
                {"ContentBasedDeduplication","true"},
                {"DeduplicationScope","messageGroup"}
            });
        }

        if (!string.IsNullOrEmpty(sqsQueueUrlOptions?.Snapshot))
        {
            queueUrlAttributes.Add(sqsQueueUrlOptions.Snapshot, new Dictionary<string, string>
            {
                {"FifoQueue","true"},
                {"ContentBasedDeduplication","true"},
                {"DeduplicationScope","messageGroup"}
            });
        }

        if (!string.IsNullOrEmpty(sqsQueueUrlOptions?.Admin))
        {
            queueUrlAttributes.Add(sqsQueueUrlOptions.Admin, new Dictionary<string, string>
            {
                {"FifoQueue","true"},
                {"ContentBasedDeduplication","true"},
                {"DeduplicationScope","messageGroup"}
            });
        }
        
        if (queueUrlAttributes.Any())
        {
            var sqsOptions = sp.GetRequiredService<SqsOptions>();
            var client = sqsOptions.CreateSqsClient();

            await client.CreateMissingQueuesAsync(queueUrlAttributes, cancellationToken);
        }
    }
}
