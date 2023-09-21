namespace RoadRegistry.BackOffice.Handlers;

using Abstractions;
using Editor.Schema;
using Editor.Schema.Extensions;
using Extensions;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public abstract class EndpointRetryableRequestHandler<TRequest, TResponse> : EndpointRequestHandler<TRequest, TResponse>
    where TRequest : EndpointRequest<TResponse>, IEndpointRetryableRequest
    where TResponse : EndpointResponse
{
    protected readonly EditorContext _context;
    private readonly IStreamStore _streamStore;
    private readonly IClock _clock;

    protected EndpointRetryableRequestHandler(CommandHandlerDispatcher dispatcher, EditorContext context, IStreamStore streamStore, IClock clock, ILogger logger) : base(dispatcher, logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    protected async Task<int> CalculateRetryAfterAsync(IEndpointRetryableRequest request, CancellationToken cancellationToken)
    {
        var projectionStateItem = await _context.ProjectionStates.SingleOrDefaultAsync(s => s.Name.Equals(WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost), cancellationToken);

        if (projectionStateItem is not null)
        {
            var currentPosition = projectionStateItem.Position;
            var lastPosition = await _streamStore.ReadHeadPosition(cancellationToken);

            if (currentPosition < lastPosition)
            {
                var eventProcessorMetrics = await _context.EventProcessorMetrics.GetMetricsAsync(WellKnownEventProcessorNames.RoadNetwork, cancellationToken);
                if (eventProcessorMetrics is not null)
                {
                    var averageTimePerEvent = eventProcessorMetrics.ElapsedMilliseconds / eventProcessorMetrics.ToPosition;
                    var estimatedTimeRemaining = averageTimePerEvent * (lastPosition - currentPosition);
                    return Convert.ToInt32(estimatedTimeRemaining) + 3 * 60 * 1000; // Added 3 minute buffer
                }
            }
        }

        return await _context.ExtractUploads
            .TookAverageProcessDuration(_clock
                .GetCurrentInstant()
                .Minus(Duration.FromDays(request.RetryAfterAverageWindowInDays)), request.DefaultRetryAfter);
    }
}
