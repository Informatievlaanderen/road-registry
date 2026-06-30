namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb;

public static class MartenExtensions
{
    public static Task IdempotentSession(this IDocumentStore store, SqsLambdaRequest sqsLambdaRequest, Func<IDocumentSession, Task> action, CancellationToken cancellationToken, ILogger? logger = null)
    {
        return store.IdempotentSession($"{sqsLambdaRequest.GetType().Name.Replace("SqsLambdaRequest", string.Empty)}-{sqsLambdaRequest.Provenance.Timestamp.ToInvariantString()}-{sqsLambdaRequest.TicketId:N}", action, cancellationToken, logger);
    }

    public static Task IdempotentSession(this IDocumentStore store, SqsRequest sqsRequest, Func<IDocumentSession, Task> action, CancellationToken cancellationToken, ILogger? logger = null)
    {
        return store.IdempotentSession($"{sqsRequest.GetType().Name.Replace("SqsRequest", string.Empty)}-{sqsRequest.ProvenanceData.Timestamp.ToInvariantString()}-{sqsRequest.TicketId:N}", action, cancellationToken, logger);
    }
}
