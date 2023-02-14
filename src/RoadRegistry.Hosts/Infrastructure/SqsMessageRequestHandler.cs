namespace RoadRegistry.Hosts.Infrastructure;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;
using Microsoft.Extensions.Logging;

public abstract class SqsMessageRequestHandler : IRequestHandler<SqsMessageRequest>
{
    private readonly ILogger<SqsMessageRequestHandler> _logger;
    private readonly IMediator _mediator;

    protected SqsMessageRequestHandler(IMediator mediator, ILogger<SqsMessageRequestHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Unit> Handle(SqsMessageRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handler started for message {request.Data?.GetType().Name}");

        var sqsLambdaRequest = ConvertSqsLambdaRequest(request.Data as SqsRequest, request.Metadata?.MessageGroupId);
        await _mediator.Send(sqsLambdaRequest, cancellationToken);

        _logger.LogInformation($"Handler finished for message {request.Data?.GetType().Name}");
        return Unit.Value;
    }

    public abstract SqsLambdaRequest ConvertSqsLambdaRequest(SqsRequest request, string groupId);
}
