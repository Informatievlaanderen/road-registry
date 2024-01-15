namespace RoadRegistry.SyncHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.StreetNameConsumer.Schema;
using StreetName;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Newtonsoft.Json;
using SqlStreamStore;
using StreetNameRecord = BackOffice.Messages.StreetNameRecord;

public class StreetNameConsumer : RoadRegistryBackgroundService
{
    private readonly IStreetNameTopicConsumer _consumer;
    private readonly IStreamStore _store;
    private readonly ILifetimeScope _container;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly KafkaOptions _options;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(StreetNameEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public StreetNameConsumer(
        ILifetimeScope container,
        KafkaOptions options,
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IStreetNameTopicConsumer consumer,
        ILogger<StreetNameConsumer> logger
    ) : base(logger)
    {
        _container = container.ThrowIfNull();
        _options = options.ThrowIfNull();
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _consumer.ConsumeContinuously(async (message, dbContext) =>
                    {
                        //TODO-rik test consumer
                        var map = _container.Resolve<EventSourcedEntityMap>();
                        var streetNamesContext = new StreetNames(map, _store, SerializerSettings, EventMapping);

                        var snapshotMessage = (SnapshotMessage)message;
                        var snapshotRecord = (StreetNameSnapshotOsloRecord)snapshotMessage.Value;

                        Logger.LogInformation("Processing streetname {Key}", snapshotMessage.Key);

                        var streetNameId = StreetNamePuri.FromValue(snapshotMessage.Key);
                        var streetNameLocalId = streetNameId.ToStreetNameLocalId();
                        var streetName = await streetNamesContext.FindAsync(streetNameLocalId, cancellationToken);

                        var streetNameRecord = new StreetNameRecord
                        {
                            PersistentLocalId = streetNameLocalId,
                            NisCode = snapshotRecord?.Gemeente.ObjectId,

                            DutchName = GetSpelling(snapshotRecord?.Straatnamen, Taal.NL),
                            FrenchName = GetSpelling(snapshotRecord?.Straatnamen, Taal.FR),
                            GermanName = GetSpelling(snapshotRecord?.Straatnamen, Taal.DE),
                            EnglishName = GetSpelling(snapshotRecord?.Straatnamen, Taal.EN),

                            DutchHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.NL),
                            FrenchHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.FR),
                            GermanHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.DE),
                            EnglishHomonymAddition = GetSpelling(snapshotRecord?.HomoniemToevoegingen, Taal.EN),

                            StreetNameStatus = snapshotRecord?.StraatnaamStatus
                        };

                        var @event = DetermineEvent(streetName, streetNameRecord);
                        await _streetNameEventWriter.WriteAsync(streetNameLocalId, new Event(@event), cancellationToken);
                        
                        //TODO-rik (koen) + ChangeRoadNetwork (command) bij delete

                        await dbContext.SaveChangesAsync(cancellationToken);

                        Logger.LogInformation("Processed streetname {Key}", snapshotMessage.Key);
                    }, cancellationToken);
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.LogError(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                const int waitSeconds = 30;
                Logger.LogCritical(ex, "Error consuming kafka events, trying again in {seconds} seconds", waitSeconds);
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), cancellationToken);
            }
        }
    }

    private static object DetermineEvent(StreetName streetNameEventSourced, StreetNameRecord streetNameRecord)
    {
        if (streetNameEventSourced is null)
        {
            return new StreetNameCreated
            {
                Record = streetNameRecord
            };
        }

        if (streetNameRecord.StreetNameStatus is null)
        {
            return new StreetNameRemoved
            {
                StreetNameId = streetNameEventSourced.StreetNameId
            };
        }

        return new StreetNameModified
        {
            Record = streetNameRecord,
            NameChanged = streetNameEventSourced.DutchName != streetNameRecord.DutchName
                          || streetNameEventSourced.EnglishName != streetNameRecord.EnglishName
                          || streetNameEventSourced.FrenchName != streetNameRecord.FrenchName
                          || streetNameEventSourced.GermanName != streetNameRecord.GermanName,
            HomonymAdditionChanged = streetNameEventSourced.DutchHomonymAddition != streetNameRecord.DutchHomonymAddition
                                     || streetNameEventSourced.EnglishHomonymAddition != streetNameRecord.EnglishHomonymAddition
                                     || streetNameEventSourced.FrenchHomonymAddition != streetNameRecord.FrenchHomonymAddition
                                     || streetNameEventSourced.GermanHomonymAddition != streetNameRecord.GermanHomonymAddition,
            StatusChanged = streetNameEventSourced.StreetNameStatus != streetNameRecord.StreetNameStatus,
            Restored = streetNameEventSourced.IsRemoved
        };
    }

    private static string? GetSpelling(List<DeseriazableGeografischeNaam>? namen, Taal taal)
    {
        return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
    }
}
