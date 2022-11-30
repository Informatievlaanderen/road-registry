namespace RoadRegistry.BackOffice;

using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Framework;
using Messages;
using SqlStreamStore;

public class ContextModule : Module
{
    public static readonly EventMapping RoadNetworkEventsEventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<IRoadRegistryContext>(context =>
        {
            var store = context.Resolve<IStreamStore>();
            var snapshotReader = context.Resolve<IRoadNetworkSnapshotReader>();
            var serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            var map = new EventSourcedEntityMap();

            return new RoadRegistryContext(map, store, snapshotReader, serializerSettings, RoadNetworkEventsEventMapping);
        });
    }
}
