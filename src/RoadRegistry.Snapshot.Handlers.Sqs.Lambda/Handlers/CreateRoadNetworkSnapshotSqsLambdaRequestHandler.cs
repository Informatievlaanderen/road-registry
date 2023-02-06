namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using BackOffice;
using MediatR;
using Microsoft.Extensions.Logging;
using Requests;

public sealed class CreateRoadNetworkSnapshotSqsLambdaRequestHandler : IRequestHandler<CreateRoadNetworkSnapshotSqsLambdaRequest>
{
    private readonly IRoadRegistryContext _context;
    private readonly ILogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler> _logger;
    private readonly Stopwatch _stopwatch;

    public CreateRoadNetworkSnapshotSqsLambdaRequestHandler(
        IRoadRegistryContext context,
        ILogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler> logger)
    {
        _stopwatch = new Stopwatch();
        _logger = logger;
    }

    public Task<Unit> Handle(CreateRoadNetworkSnapshotSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        try
        {
            _logger.LogInformation("Create snapshot started for new message received from SQS");

            // Get latest snapshost
            // EG snapshot is version 200

            // Get messages
            // Message count is eg 104

            // Snapshotstrategy says per 50 (host has been down for a while
            // 4 sqs messages

            // Snapshot 200 + 104 messages retreived, only play 100
            // Take new snapshot

            // Remaining 3 sqs messages
            // Snapshot 300 + 4 messages -> do nothing

            _logger.LogInformation("Create snapshot completed in {TotalElapsedTimespan}", _stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create snapshot failed in {TotalElapsedTimespan}", _stopwatch.Elapsed);
        }
        finally
        {
            _stopwatch.Stop();
        }

        return Task.FromResult(Unit.Value);
    }
}