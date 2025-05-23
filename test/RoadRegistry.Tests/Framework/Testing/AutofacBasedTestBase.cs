namespace RoadRegistry.Tests.Framework.Testing;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.Comparers;
using Be.Vlaanderen.Basisregisters.AggregateSource.Testing.SqlStreamStore.Autofac;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Framework;
using LogExtensions = Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing.LogExtensions;

public abstract class AutofacBasedTestBase
{
    private readonly Lazy<IContainer> _container;

    protected AutofacBasedTestBase(ITestOutputHelper testOutputHelper)
    {
        _container = new Lazy<IContainer>(() =>
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => new EventSourcedEntityMap());
            containerBuilder.RegisterModule(new SqlStreamStoreModule());

            var services = new ServiceCollection();
            ConfigureServices(services);
            containerBuilder.Populate(services);
            ConfigureEventHandling(containerBuilder);
            ConfigureCommandHandling(containerBuilder);
            ConfigureContainer(containerBuilder);

            containerBuilder.UseAggregateSourceTesting(CreateFactComparer(), CreateExceptionComparer());

            containerBuilder.RegisterInstance(testOutputHelper);
            containerBuilder.RegisterType<XUnitLogger>().AsImplementedInterfaces();
            containerBuilder.RegisterInstance(new NullLoggerFactory()).As<ILoggerFactory>();

            return containerBuilder.Build();
        });

        LogExtensions.LogSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
    }

    protected IContainer Container => _container.Value;
    protected IExceptionCentricTestSpecificationRunner ExceptionCentricTestSpecificationRunner => Container.Resolve<IExceptionCentricTestSpecificationRunner>();
    protected IEventCentricTestSpecificationRunner EventCentricTestSpecificationRunner => Container.Resolve<IEventCentricTestSpecificationRunner>();
    protected IFactComparer FactComparer => Container.Resolve<IFactComparer>();
    protected IExceptionComparer ExceptionComparer => Container.Resolve<IExceptionComparer>();
    protected ILogger Logger => Container.Resolve<ILogger>();

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }
    protected virtual void ConfigureContainer(ContainerBuilder containerBuilder)
    {
    }
    protected abstract void ConfigureCommandHandling(ContainerBuilder builder);
    protected abstract void ConfigureEventHandling(ContainerBuilder builder);

    protected virtual IExceptionComparer CreateExceptionComparer()
    {
        var comparer = new CompareLogic();
        comparer.Config.MembersToIgnore.Add("Source");
        comparer.Config.MembersToIgnore.Add("StackTrace");
        comparer.Config.MembersToIgnore.Add("TargetSite");
        return new CompareNetObjectsBasedExceptionComparer(comparer);
    }

    protected virtual IFactComparer CreateFactComparer()
    {
        var comparer = new CompareLogic();
        comparer.Config.MembersToIgnore.Add("Provenance");
        return new CompareNetObjectsBasedFactComparer(comparer);
    }
}
