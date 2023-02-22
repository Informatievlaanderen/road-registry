namespace RoadRegistry.Snapshot.Handlers.Sqs;

using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using RoadRegistry.BackOffice.Configuration;

public sealed class SnapshotSqsHandlersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .Register(c => new SnapshotSqsQueue(c.Resolve<SqsOptions>(), c.Resolve<SqsQueueUrlOptions>()))
            .SingleInstance();
    }
}
