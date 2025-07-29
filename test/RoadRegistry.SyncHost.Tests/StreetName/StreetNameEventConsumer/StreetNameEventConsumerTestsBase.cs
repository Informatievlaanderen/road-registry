namespace RoadRegistry.SyncHost.Tests.StreetName.StreetNameEventConsumer;

using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Editor.Schema;
using Editor.Schema.Organizations;
using Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using NodaTime.Testing;
using SqlStreamStore;
using Sync.StreetNameRegistry;
using SyncHost.StreetName;

public class StreetNameEventConsumerTestsBase
{
    protected (StreetNameEventConsumer, IStreamStore, InMemoryStreetNameEventTopicConsumer) BuildSetup(
        Action<StreetNameEventConsumerContext>? configureDbContext = null,
        Action<EditorContext>? configureEditorContext = null
    )
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => new EventSourcedEntityMap());
        containerBuilder.Register(_ => new ConfigurationBuilder().Build()).As<IConfiguration>();
        containerBuilder.Register(_ => new LoggerFactory()).As<ILoggerFactory>();
        containerBuilder
            .Register(c => new FakeRoadNetworkIdGenerator())
            .As<IRoadNetworkIdGenerator>()
            .SingleInstance();

        containerBuilder
            .RegisterDbContext<StreetNameEventConsumerContext>(string.Empty,
                _ => { }
                , dbContextOptionsBuilder =>
                {
                    var context = new StreetNameEventConsumerContext(dbContextOptionsBuilder.Options);
                    configureDbContext?.Invoke(context);
                    return context;
                }
            );
        containerBuilder
            .RegisterDbContext<EditorContext>(string.Empty,
                _ => { }
                , dbContextOptionsBuilder =>
                {
                    var context = new EditorContext(dbContextOptionsBuilder.Options);

                    context.ProjectionStates.Add(new ProjectionStateItem
                    {
                        Name = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
                        Position = -1
                    });
                    context.ProjectionStates.Add(new ProjectionStateItem
                    {
                        Name = WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost,
                        Position = -1
                    });

                    context.OrganizationsV2.Add(new OrganizationRecordV2
                    {
                        Id = 1,
                        Code = OrganizationOvoCode.DigitaalVlaanderen,
                        OvoCode = OrganizationOvoCode.DigitaalVlaanderen,
                        Name = "Digitaal Vlaanderen"
                    });

                    configureEditorContext?.Invoke(context);
                    return context;
                }
            );

        var lifetimeScope = containerBuilder.Build();

        var store = new InMemoryStreamStore();
        var topicConsumer = new InMemoryStreetNameEventTopicConsumer(lifetimeScope.Resolve<StreetNameEventConsumerContext>);
        var eventEnricher = EnrichEvent.WithTime(new FakeClock(NodaConstants.UnixEpoch));

        return (new StreetNameEventConsumer(
            store,
            new StreetNameEventWriter(store, eventEnricher),
            new RoadNetworkEventWriter(store, eventEnricher),
            topicConsumer,
            lifetimeScope.Resolve<EditorContext>,
            new NullLoggerFactory().CreateLogger<StreetNameEventConsumer>()
        ), store, topicConsumer);
    }
}
