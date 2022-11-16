using Amazon;
using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

namespace RoadRegistry.BackOffice.Handlers.Sqs
{
    using Microsoft.Extensions.Logging;

    public sealed class SqsHandlersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new SqsOptions(RegionEndpoint.EUWest1, EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .SingleInstance();

            builder
                .Register(c => new SqsQueuePublisher(c.Resolve<SqsOptions>(), c.Resolve<ILogger<SqsQueuePublisher>>()))
                .As<ISqsQueuePublisher>()
                .SingleInstance();

            builder
                .Register(c => new SqsQueueConsumer(c.Resolve<SqsOptions>(), c.Resolve<ILogger<SqsQueueConsumer>>()))
                .As<ISqsQueueConsumer>()
                .SingleInstance();
        }
    }
}
