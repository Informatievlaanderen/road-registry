namespace RoadRegistry.Legacy.Extract.Readers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using BackOffice.Core;
    using BackOffice.Messages;
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
                    new ImportedMunicipalitiesReader(clock, wkbReader,
                        loggerFactory.CreateLogger<ImportedMunicipalitiesReader>()),
                    TimedEventReader.DefaultThreshold,
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedOrganizationsReader(clock, loggerFactory.CreateLogger<ImportedOrganizationsReader>()),
                    100,
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedRoadNodesReader(clock, wkbReader,
                        loggerFactory.CreateLogger<ImportedRoadNodesReader>()),
                    TimedEventReader.DefaultThreshold,
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedRoadSegmentsReader(clock, wkbReader,
                        loggerFactory.CreateLogger<ImportedRoadSegmentsReader>()),
                    TimedEventReader.DefaultThreshold,
                    loggerFactory.CreateLogger<TimedEventReader>()
                ),
                new TimedEventReader(
                    new ImportedGradeSeparatedJunctionsReader(clock,
                        loggerFactory.CreateLogger<ImportedGradeSeparatedJunctionsReader>()),
                    100,
                    loggerFactory.CreateLogger<TimedEventReader>()
                )
            );
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
