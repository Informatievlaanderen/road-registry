namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Aiv.Vbr.AggregateSource;
    using Aiv.Vbr.EventHandling;
    using FluentValidation;
    using Framework;
    using Newtonsoft.Json;
    using SqlStreamStore;

    internal static class CommandHandlerModulePipelines
    {
        public static ICommandHandlerBuilder<TCommand> ValidateUsing<TCommand>(
            this ICommandHandlerBuilder<TCommand> builder, IValidator<TCommand> validator)
        {
            return builder.Pipe(next => async (message, ct) =>
            {
                await validator.ValidateAndThrowAsync(message.Body, cancellationToken: ct);
                await next(message, ct);
            });
        }
    }

    public interface ISession
    {
        IRoadNetworks RoadNetworks { get; }
    }

    public class Session : ISession
    {
        public Session(ConcurrentUnitOfWork unitOfWork, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        {
            RoadNetworks = new RoadNetworks(unitOfWork, store, settings, mapping);
        }

        public IRoadNetworks RoadNetworks { get; }
    }

    public class EventSourceMap
    {
        private readonly ConcurrentDictionary<StreamName, EventSourceEntry> _entries;

        public EventSourceMap() => _entries = new ConcurrentDictionary<StreamName, EventSourceEntry>();

        public void Attach(EventSourceEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (!_entries.TryAdd(entry.Stream, entry))
                throw new ArgumentException($"The event source of stream {entry.Stream} was already attached.");
        }

        public bool TryGet(StreamName stream, out EventSourceEntry entry) => _entries.TryGetValue(stream, out entry);

        public EventSourceEntry[] Entries => 
    }

    public class EventSourceEntry
    {
        public EventSourceEntry(IEventSource source, StreamName stream, int expectedVersion)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Stream = stream;
            ExpectedVersion = expectedVersion;
        }

        public StreamName Stream { get; }
        public IEventSource Source { get; }
        public int ExpectedVersion { get; }
    }
}
