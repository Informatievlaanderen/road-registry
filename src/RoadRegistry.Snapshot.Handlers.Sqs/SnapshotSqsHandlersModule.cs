namespace RoadRegistry.Snapshot.Handlers.Sqs;

using Autofac;
using BackOffice.Configuration;
using BackOffice.Handlers.Sqs;

public sealed class SnapshotSqsHandlersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .Register(c => new SnapshotSqsQueue(c.Resolve<ISqsQueueFactory>(), c.Resolve<SqsQueueUrlOptions>()))
            .SingleInstance();
    }
}
