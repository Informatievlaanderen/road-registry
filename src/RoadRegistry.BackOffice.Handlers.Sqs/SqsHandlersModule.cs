using Amazon;
using Autofac;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

namespace RoadRegistry.BackOffice.Handlers.Sqs
{
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public sealed class SqsHandlersModule : Module
    {
        internal const string SqsQueueUrlConfigKey = "SqsQueueUrl";

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

            builder.Register(c =>
                {
                    var configuration = c.Resolve<IConfiguration>();

                    var sqsOptions = c.Resolve<SqsOptions>();
                    var sqsQueueUrl = configuration.GetValue<string>(SqsQueueUrlConfigKey);
                    var sqsQueue = new SqsQueue(sqsOptions, sqsQueueUrl);

                    return sqsQueue;
                })
                .As<ISqsQueue>()
                .SingleInstance();
        }
    }
}
