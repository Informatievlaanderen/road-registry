using System.Text.Json;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using BackOffice;
using Hosts;

public class Function : RoadRegistryLambdaFunction
{
    public Function() : base("RoadRegistry.SnapshotCache.Lambda", new[] { typeof(DomainAssemblyMarker).Assembly })
    {
    }

    public async Task FunctionHandler(SQSEvent @event, ILambdaContext context)
    {
        foreach (var message in @event.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    /// <summary>
    ///     Process message as an asynchronous operation.
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