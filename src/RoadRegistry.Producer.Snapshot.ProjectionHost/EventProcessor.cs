namespace RoadRegistry.Producer.Snapshot.ProjectionHost
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Hosts;
    using Hosts.Metadata;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Schema;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class EventProcessor
    {
        private const int CatchUpBatchSize = 1; //Cannot be batched since it's produced to Kafka
        private const string QueueName = "roadregistry-producer-snapshot-projectionhost";

        public static readonly EventMapping EventMapping =
            new(
                new List<IReadOnlyDictionary<string, Type>>
                {
                    EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly)
                }.SelectMany(dict => dict).ToDictionary(x => x.Key, x => x.Value));

        public static readonly JsonSerializerSettings SerializerSettings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        private readonly Func<ProducerSnapshotContext> _dbContextFactory;
        private readonly EnvelopeFactory _envelopeFactory;
        private readonly AcceptStreamMessageFilter _filter;
        private readonly ILogger<EventProcessor> _logger;
        private readonly IMetadataUpdater _metadataUpdater;
        private readonly ConnectedProjectionHandlerResolver<ProducerSnapshotContext> _resolver;
        private readonly IStreamStore _streamStore;

        public EventProcessor(
            IStreamStore streamStore,
            AcceptStreamMessageFilter filter,
            EnvelopeFactory envelopeFactory,
            ConnectedProjectionHandlerResolver<ProducerSnapshotContext> resolver,
            Func<ProducerSnapshotContext> dbContextFactory,
            ILogger<EventProcessor> logger,
            IMetadataUpdater metadataUpdater)
        {
            _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _envelopeFactory = envelopeFactory ?? throw new ArgumentNullException(nameof(envelopeFactory));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metadataUpdater = metadataUpdater ?? throw new ArgumentNullException(nameof(metadataUpdater));
        }

        private async Task CatchUp(long? after, int catchUpBatchSize, CancellationToken token)
        {
            _logger.LogInformation("Catching up as of {Position}", after ?? -1L);
            var observedMessageCount = 0;
            var catchUpPosition = after ?? Position.Start;
            var context = _dbContextFactory();
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var page = await _streamStore
                .ReadAllForwards(
                    catchUpPosition,
                    catchUpBatchSize,
                    true,
                    token)
                .ConfigureAwait(false);

            while (page.Messages.Length > 0)
            {
                foreach (var streamMessage in page.Messages)
                {
                    if (after.HasValue &&
                        streamMessage.Position == after.Value)
                        continue; // skip already processed message

                    if (_filter(streamMessage))
                    {
                        _logger.LogInformation("Catching up on {MessageType} at {Position}",
                            streamMessage.Type, streamMessage.Position);
                        var envelope = _envelopeFactory.Create(streamMessage);
                        var handlers = _resolver(envelope);
                        foreach (var handler in handlers)
                            await handler
                                .Handler(context, envelope, token)
                                .ConfigureAwait(false);
                    }

                    observedMessageCount++;
                    catchUpPosition = streamMessage.Position;

                    if (observedMessageCount % CatchUpBatchSize == 0)
                    {
                        _logger.LogInformation(
                            "Flushing catch up position of {0} and persisting changes ...",
                            catchUpPosition);
                        await context
                            .UpdateProjectionState(
                                QueueName,
                                catchUpPosition,
                                token)
                            .ConfigureAwait(false);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(token).ConfigureAwait(false);
                        await context.DisposeAsync().ConfigureAwait(false);

                        context = _dbContextFactory();
                        context.ChangeTracker.AutoDetectChangesEnabled = false;
                        observedMessageCount = 0;
                    }
                }

                page = await page.ReadNext(token).ConfigureAwait(false);
            }

            if (observedMessageCount > 0) // case where we just read the last page and pending work in memory needs to be flushed
            {
                _logger.LogInformation(
                    "Flushing catch up position of {Position} and persisting changes ...",
                    catchUpPosition);
                await context
                    .UpdateProjectionState(
                        QueueName,
                        catchUpPosition,
                        token)
                    .ConfigureAwait(false);
                context.ChangeTracker.DetectChanges();
                await context.SaveChangesAsync(token).ConfigureAwait(false);
            }

            await context.DisposeAsync().ConfigureAwait(false);
        }

        public async Task Resume(CancellationToken token)
        {
            _logger.LogInformation("Resuming ...");

            await using var resumeContext = _dbContextFactory();
            var projection =
                await resumeContext.ProjectionStates
                    .SingleOrDefaultAsync(
                        item => item.Name == QueueName,
                        token)
                    .ConfigureAwait(false);
            try
            {
                await CatchUp(projection?.Position, CatchUpBatchSize, token);
            }
            finally
            {
                await _metadataUpdater.UpdateAsync(token).ConfigureAwait(false);
            }
        }
    }
}
