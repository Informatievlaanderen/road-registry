namespace RoadRegistry.Hosts.Infrastructure.Modules;

using Autofac;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;

public class RoadNetworkSnapshotModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context => new RoadNetworkSnapshotReaderWriter(
                new RoadNetworkSnapshotsBlobClient(
                    new SqlBlobClient(
                        new SqlConnectionStringBuilder(
                            context
                                .Resolve<IConfiguration>()
                                .GetConnectionString(WellknownConnectionNames.Snapshots)
                        ), WellknownSchemas.SnapshotSchema)),
                context.Resolve<RecyclableMemoryStreamManager>()))
            .SingleInstance();

        builder.RegisterInstance(new RecyclableMemoryStreamManager());

        builder.Register<IRoadNetworkSnapshotReader>(context => context.Resolve<RoadNetworkSnapshotReaderWriter>())
            .SingleInstance();
        builder.Register<IRoadNetworkSnapshotWriter>(context => context.Resolve<RoadNetworkSnapshotReaderWriter>())
            .SingleInstance();
    }
}
