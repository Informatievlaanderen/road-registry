namespace RoadRegistry.LegacyStreamExtraction.Readers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using BackOffice.Messages;
    using BackOffice.Model;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
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

        public IEnumerable<StreamEvent> ReadEvents(SqlConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            yield return new StreamEvent(RoadNetworks.Stream, new BeganRoadNetworkImport
            {
                When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
            });

            foreach(var @event in _reader.ReadEvents(connection))
                yield return @event;

            yield return new StreamEvent(RoadNetworks.Stream, new CompletedRoadNetworkImport
            {
                When = InstantPattern.ExtendedIso.Format(_clock.GetCurrentInstant())
            });
        }
    }
}
