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
    protected EditorContext Context { get; }
    private readonly IStreamStore _streamStore;
    private readonly IClock _clock;

    protected EndpointRetryableRequestHandler(CommandHandlerDispatcher dispatcher, EditorContext context, IStreamStore streamStore, IClock clock, ILogger logger) : base(dispatcher, logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    protected async Task<int> CalculateRetryAfterAsync(IEndpointRetryableRequest request, CancellationToken cancellationToken)
    {
        var projectionStateItem = await Context.ProjectionStates.SingleOrDefaultAsync(s => s.Name.Equals(WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost), cancellationToken);

        if (projectionStateItem is not null)
        {
            var currentPosition = projectionStateItem.Position;
            var maxPosition = await _streamStore.ReadHeadPosition(cancellationToken);

            if (currentPosition < maxPosition)
            {
                var eventProcessorMetrics = await Context.EventProcessorMetrics.GetMetricsAsync(WellKnownEventProcessorNames.RoadNetwork, currentPosition, cancellationToken);
                if (eventProcessorMetrics is not null)
                {
                    var averageTimePerEvent = eventProcessorMetrics.ElapsedMilliseconds / eventProcessorMetrics.ToPosition;
                    var estimatedTimeRemaining = averageTimePerEvent * (maxPosition - currentPosition);
                    return Convert.ToInt32(estimatedTimeRemaining) + 5 * 60 * 1000; // Added 5 minute buffer
                }
            }
        }

        return await Context.ExtractUploads
            .TookAverageProcessDuration(_clock
                .GetCurrentInstant()
                .Minus(Duration.FromDays(request.RetryAfterAverageWindowInDays)), request.DefaultRetryAfter);
    }
}
