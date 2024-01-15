namespace RoadRegistry.SyncHost;

using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using Sync.StreetNameRegistry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StreetName;
using StreetNameRecord = BackOffice.Messages.StreetNameRecord;

public class StreetNameConsumer : RoadRegistryBackgroundService
{
    private readonly IStreetNameTopicConsumer _consumer;
    private readonly IStreamStore _store;
    private readonly ILifetimeScope _container;
    private readonly IStreetNameEventWriter _streetNameEventWriter;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(StreetNameEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public StreetNameConsumer(
        ILifetimeScope container,
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IStreetNameTopicConsumer consumer,
        ILogger<StreetNameConsumer> logger
    ) : base(logger)
    {
        _container = container.ThrowIfNull();
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
                        Logger.LogInformation("Processing streetname {Key}", message.Key);

                        var map = _container.Resolve<EventSourcedEntityMap>();
                        var streetNamesContext = new StreetNames(map, _store, SerializerSettings, EventMapping);

                        var snapshotRecord = (StreetNameSnapshotOsloRecord)message.Value;
                        var streetNameId = StreetNamePuri.FromValue(message.Key);
                        var streetNameLocalId = streetNameId.ToStreetNameLocalId();

                        var streetNameEventSourced = await streetNamesContext.FindAsync(streetNameLocalId, cancellationToken);

                        var streetNameDbRecord = new StreetNameRecord
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

                        var @event = DetermineEvent(streetNameEventSourced, streetNameDbRecord);
                        await _streetNameEventWriter.WriteAsync(streetNameLocalId, new Event(@event), cancellationToken);
                        
                        //TODO-rik (koen) + ChangeRoadNetwork (command) bij delete

                        await dbContext.SaveChangesAsync(cancellationToken);

                        Logger.LogInformation("Processed streetname {Key}", message.Key);
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

    private static object DetermineEvent(StreetName streetNameEventSourced, StreetNameRecord streetNameDbRecord)
    {
        if (streetNameEventSourced is null)
        {
            return new StreetNameCreated
            {
                Record = streetNameDbRecord
            };
        }

        if (streetNameDbRecord.StreetNameStatus is null)
        {
            return new StreetNameRemoved
            {
                StreetNameId = streetNameEventSourced.StreetNameId
            };
        }

        return new StreetNameModified
        {
            Record = streetNameDbRecord,
            NameChanged = streetNameEventSourced.DutchName != streetNameDbRecord.DutchName
                          || streetNameEventSourced.EnglishName != streetNameDbRecord.EnglishName
                          || streetNameEventSourced.FrenchName != streetNameDbRecord.FrenchName
                          || streetNameEventSourced.GermanName != streetNameDbRecord.GermanName,
            HomonymAdditionChanged = streetNameEventSourced.DutchHomonymAddition != streetNameDbRecord.DutchHomonymAddition
                                     || streetNameEventSourced.EnglishHomonymAddition != streetNameDbRecord.EnglishHomonymAddition
                                     || streetNameEventSourced.FrenchHomonymAddition != streetNameDbRecord.FrenchHomonymAddition
                                     || streetNameEventSourced.GermanHomonymAddition != streetNameDbRecord.GermanHomonymAddition,
            StatusChanged = streetNameEventSourced.StreetNameStatus != streetNameDbRecord.StreetNameStatus,
            Restored = streetNameEventSourced.IsRemoved
        };
    }

    private static string GetSpelling(List<DeseriazableGeografischeNaam> namen, Taal taal)
    {
        return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
    }
}
