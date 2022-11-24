namespace RoadRegistry.BackOffice;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Framework;
using MediatR;
using MediatR.Pipeline;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;
using Module = Autofac.Module;

public class MediatorModule : Module
{
    private readonly IEnumerable<Type> _mediatorOpenTypes = new[]
    {
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>),
        typeof(INotificationHandler<>),
        typeof(IStreamRequestHandler<,>)
    };

    private static IEnumerable<Assembly> DetermineAvailableAssemblyCollection()
    {
        var executorAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executorDirectoryInfo = new DirectoryInfo(executorAssemblyLocation).Parent;
        var assemblyFileInfoCollection = executorDirectoryInfo.EnumerateFiles("RoadRegistry.*.dll");
        var assemblyCollection = assemblyFileInfoCollection.Select(fi => Assembly.LoadFrom(fi.FullName));
        return assemblyCollection.ToList();
    }

    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();
        builder.RegisterGeneric(typeof(ValidationPipelineBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        builder.Register<ServiceFactory>(ctx =>
        {
            var c = ctx.Resolve<IComponentContext>();
            return t => c.Resolve(t);
        });

        builder.Register<IRoadRegistryContext>(context =>
        {
            var store = context.Resolve<IStreamStore>();
            var snapshotReader = context.Resolve<IRoadNetworkSnapshotReader>();
            var serializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            var eventMapping = new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

            var map = new EventSourcedEntityMap();
            return new RoadRegistryContext(map, store, snapshotReader, serializerSettings, eventMapping);
        });

        RegisterAvailableAssemblyModules(builder);
    }

    private void RegisterAvailableAssemblyModules(ContainerBuilder builder)
    {
        var availableModuleAssemblyCollection = DetermineAvailableAssemblyCollection().ToList();
        availableModuleAssemblyCollection.Remove(GetType().Assembly);

        foreach (var assembly in availableModuleAssemblyCollection)
        {
            foreach (var mediatrOpenType in _mediatorOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    .AsImplementedInterfaces();
            }
        }
    }
}
