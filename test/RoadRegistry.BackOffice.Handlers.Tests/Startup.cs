namespace RoadRegistry.BackOffice.Handlers.Tests;

using Autofac;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Core;
using FeatureCompare;
using FeatureCompare.V1.Readers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice.FeatureToggles;
using SqlStreamStore;
using DomainAssemblyMarker = BackOffice.DomainAssemblyMarker;

public class Startup : TestStartup
{
    protected override CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp)
    {
        return Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                    sp.GetService<ITransactionZoneZipArchiveReader>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IClock>(),
                    new FakeExtractUploadFailedEmailClient(),
                    new UseExtractsV2FeatureToggle(true),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }));
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModulesFromAssemblyContaining<DomainAssemblyMarker>();
        builder.RegisterModulesFromAssemblyContaining<Handlers.DomainAssemblyMarker>();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
    }
}
