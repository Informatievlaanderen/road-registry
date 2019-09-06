namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using BackOffice.Model;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using NodaTime.Text;

    public class LegacyEventReader : IEventReader
    {
        private readonly IClock _clock;
        private readonly IEventReader _reader;

        public LegacyEventReader(IClock clock, WellKnownBinaryReader wkbReader, ILoggerFactory loggerFactory)
        {
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (wkbReader == null) throw new ArgumentNullException(nameof(wkbReader));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _clock = clock;
            _reader = new CompositeEventReader(
                new TimedEventReader(
                    new ImportedOrganizationsReader(clock, loggerFactory.CreateLogger<ImportedOrganizationsReader>()),
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedRoadNodesReader(clock, wkbReader, loggerFactory.CreateLogger<ImportedRoadNodesReader>()),
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedRoadSegmentsReader(clock, wkbReader, loggerFactory.CreateLogger<ImportedRoadSegmentsReader>()),
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedGradeSeparatedJunctionsReader(clock, loggerFactory.CreateLogger<ImportedGradeSeparatedJunctionsReader>()),
                    loggerFactory.CreateLogger<TimedEventReader>()
                ));
        }

        public async Task<IReadOnlyCollection<RecordedEvent>> ReadAsync(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            return ImmutableList<RecordedEvent>.Empty
                .Add(new RecordedEvent(RoadNetworks.Stream, new BeganRoadNetworkImport
                {
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }))
                .AddRange(await _reader.ReadAsync(connection))
                .Add(new RecordedEvent(RoadNetworks.Stream, new CompletedRoadNetworkImport
                {
                    When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
                }));
        }
    }
}
