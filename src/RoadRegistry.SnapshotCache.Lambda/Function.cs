using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.SnapshotCache.Lambda;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using BackOffice.Handlers.Sqs;
using Hosts;

public class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.SnapshotCache.Lambda", new[] { typeof(DomainAssemblyMarker).Assembly })
    {
    }

    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    /// <summary>
    /// Process message as an asynchronous operation.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="context">The context.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processed message {message.Body}");
        await Task.CompletedTask;
    }
}
