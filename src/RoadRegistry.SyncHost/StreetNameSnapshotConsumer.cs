namespace RoadRegistry.SyncHost;

using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Hosts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using StreetName;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StreetNameRecord = BackOffice.Messages.StreetNameRecord;

public class StreetNameSnapshotConsumer : RoadRegistryBackgroundService
{
    private readonly IStreetNameSnapshotTopicConsumer _consumer;
    private readonly IStreamStore _store;
    private readonly ILifetimeScope _container;
    private readonly IStreetNameEventWriter _streetNameEventWriter;
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(StreetNameEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public StreetNameSnapshotConsumer(
        ILifetimeScope container,
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IStreetNameSnapshotTopicConsumer consumer,
        ILogger<StreetNameSnapshotConsumer> logger
    ) : base(logger)
    {
        _container = container.ThrowIfNull();
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _roadNetworkCommandQueue = roadNetworkCommandQueue.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
            {
                Logger.LogInformation("Processing streetname {Key}", message.Key);

                var map = _container.Resolve<EventSourcedEntityMap>();
                var streetNamesContext = new StreetNames(map, _store, SerializerSettings, EventMapping);

                var snapshotRecord = (StreetNameSnapshotRecord)message.Value;
                var streetNameId = StreetNamePuri.FromValue(message.Key);
                var streetNameLocalId = streetNameId.ToStreetNameLocalId();

                var streetNameEventSourced = await streetNamesContext.FindAsync(streetNameLocalId, cancellationToken);

                var streetNameDbRecord = new StreetNameRecord
                {
                    StreetNameId = streetNameId,
                    PersistentLocalId = streetNameLocalId,
                    NisCode = snapshotRecord?.Gemeente?.ObjectId,

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

                if (@event is StreetNameRemoved)
                {
                    await _roadNetworkCommandQueue.WriteAsync(new Command(new UnlinkRoadSegmentsFromStreetName
                    {
                        Id = streetNameLocalId
                    }), cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);

                Logger.LogInformation("Processed streetname {Key}", message.Key);
            }, cancellationToken);
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
            NameModified = streetNameEventSourced.DutchName != streetNameDbRecord.DutchName
                           || streetNameEventSourced.EnglishName != streetNameDbRecord.EnglishName
                           || streetNameEventSourced.FrenchName != streetNameDbRecord.FrenchName
                           || streetNameEventSourced.GermanName != streetNameDbRecord.GermanName,
            HomonymAdditionModified = streetNameEventSourced.DutchHomonymAddition != streetNameDbRecord.DutchHomonymAddition
                                     || streetNameEventSourced.EnglishHomonymAddition != streetNameDbRecord.EnglishHomonymAddition
                                     || streetNameEventSourced.FrenchHomonymAddition != streetNameDbRecord.FrenchHomonymAddition
                                     || streetNameEventSourced.GermanHomonymAddition != streetNameDbRecord.GermanHomonymAddition,
            StatusModified = streetNameEventSourced.StreetNameStatus != streetNameDbRecord.StreetNameStatus,
            Restored = streetNameEventSourced.IsRemoved
        };
    }

    private static string GetSpelling(List<DeseriazableGeografischeNaam> namen, Taal taal)
    {
        return namen?.SingleOrDefault(x => x.Taal == taal)?.Spelling;
    }
}
