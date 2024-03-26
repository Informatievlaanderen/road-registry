namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Configuration;
using Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Uploads;

public sealed class SqsHandlersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .Register(c =>
            {
                var configuration = c.Resolve<IConfiguration>();
                var jsonSerializerSettings = SqsJsonSerializerSettingsProvider.CreateSerializerSettings();
                
                var sqsConfiguration = configuration.GetOptions<SqsConfiguration>();
                if (sqsConfiguration?.ServiceUrl != null)
                {
                    return new DevelopmentSqsOptions(jsonSerializerSettings, sqsConfiguration.ServiceUrl);
                }
                
                return new SqsOptions(jsonSerializerSettings);
            })
            .As<SqsOptions>()
            .SingleInstance();

        builder
            .Register(c => new SqsJsonMessageSerializer(c.Resolve<SqsOptions>()))
            .SingleInstance();

        builder
            .Register<ISqsQueueFactory>(c =>
            {
                var hostEnvironment = c.Resolve<IHostEnvironment>();
                if (hostEnvironment.IsDevelopment())
                {
                    return new FakeSqsQueueFactory(c.Resolve<SqsJsonMessageSerializer>(), c.Resolve<ILoggerFactory>());
                }

                return new SqsQueueFactory(c.Resolve<SqsOptions>());
            })
            .SingleInstance();

        builder
            .Register(c => new SqsQueuePublisher(c.Resolve<SqsOptions>(), c.Resolve<ILogger<SqsQueuePublisher>>()))
            .As<ISqsQueuePublisher>()
            .SingleInstance();

        builder
            .Register(c => new SqsQueueConsumer(c.Resolve<SqsOptions>(), c.Resolve<ILogger<SqsQueueConsumer>>()))
            .As<ISqsQueueConsumer>()
            .SingleInstance();

        builder
            .RegisterOptions<SqsQueueUrlOptions>();

        builder
            .Register(c => new SqsMessagesBlobClient(c.Resolve<IBlobClientFactory>().Create(WellKnownBuckets.SqsMessagesBucket), c.Resolve<SqsJsonMessageSerializer>()))
            .SingleInstance();

        builder
            .Register(c => new BackOfficeS3SqsQueue(c.Resolve<ISqsQueueFactory>(), c.Resolve<SqsJsonMessageSerializer>(), c.Resolve<SqsQueueUrlOptions>(), c.Resolve<SqsMessagesBlobClient>(), c.Resolve<ILogger<BackOfficeS3SqsQueue>>()))
            .As<IBackOfficeS3SqsQueue>()
            .SingleInstance();

        builder
            .Register(c => new AdminSqsQueue(c.Resolve<ISqsQueueFactory>(), c.Resolve<SqsQueueUrlOptions>()))
            .As<IAdminSqsQueue>()
            .SingleInstance();
    }
}
