namespace RoadRegistry.SyncHost.StreetName;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Hosts;
using RoadRegistry.StreetName;
using SqlStreamStore;
using Sync.StreetNameRegistry;
using StreetNameRecord = BackOffice.Messages.StreetNameRecord;

public class StreetNameSnapshotConsumer : RoadRegistryBackgroundService
{
    private readonly IStreetNameSnapshotTopicConsumer _consumer;
    private readonly IStreamStore _store;
    private readonly ILifetimeScope _container;
    private readonly IStreetNameEventWriter _streetNameEventWriter;

    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    private static readonly JsonSerializerSettings SerializerSettings =
        EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

    public StreetNameSnapshotConsumer(
        ILifetimeScope container,
        IStreamStore store,
        IStreetNameEventWriter streetNameEventWriter,
        IStreetNameSnapshotTopicConsumer consumer,
        ILogger<StreetNameSnapshotConsumer> logger
    ) : base(logger)
    {
        _container = container.ThrowIfNull();
        _store = store.ThrowIfNull();
        _streetNameEventWriter = streetNameEventWriter.ThrowIfNull();
        _consumer = consumer.ThrowIfNull();
    }

    protected override async Task ExecutingAsync(CancellationToken cancellationToken)
    {
        await _consumer.ConsumeContinuously(async (message, dbContext) =>
        {
            await ConsumeHandler(message, dbContext);
        }, cancellationToken);
    }

    private async Task ConsumeHandler(SnapshotMessage message, StreetNameSnapshotConsumerContext dbContext)
    {
        Logger.LogInformation("Processing streetname {Key}", message.Key);

        var map = _container.Resolve<EventSourcedEntityMap>();
        var streetNamesContext = new StreetNames(map, _store, SerializerSettings, EventMapping);

        var snapshotRecord = (StreetNameSnapshotRecord)message.Value;
        var streetNameId = new StreetNameId(message.Key);
        var streetNameLocalId = streetNameId.ToStreetNameLocalId();

        var streetNameEventSourced = await streetNamesContext.FindAsync(streetNameLocalId);

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
        await _streetNameEventWriter.WriteAsync(streetNameLocalId, new Event(@event), CancellationToken.None);

        await dbContext.SaveChangesAsync(CancellationToken.None);

        Logger.LogInformation("Processed streetname {Key}", message.Key);
    }

    private static object DetermineEvent(StreetName streetNameEventSourced, StreetNameRecord streetNameDbRecord)
    {
        if (streetNameDbRecord.StreetNameStatus is null)
        {
            return new StreetNameRemoved
            {
                StreetNameId = streetNameDbRecord.StreetNameId
            };
        }

        if (streetNameEventSourced is null)
        {
            return new StreetNameCreated
            {
                Record = streetNameDbRecord
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
