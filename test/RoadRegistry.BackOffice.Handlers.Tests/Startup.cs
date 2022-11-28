namespace RoadRegistry.BackOffice.Handlers.Tests;

using Autofac;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using SqlStreamStore;
using MediatorModule = BackOffice.MediatorModule;

public class Startup : TestStartup
{
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<MediatorModule>();
        builder.RegisterModule<Handlers.MediatorModule>();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
    }

    protected override CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp)
    {
        return Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                    sp.GetService<IClock>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IRoadNetworkSnapshotWriter>(),
                    sp.GetService<IClock>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                    sp.GetService<IClock>()
                )
            }));
    }
}
