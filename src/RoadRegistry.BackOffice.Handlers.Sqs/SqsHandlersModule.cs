namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Amazon;
using Autofac;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        builder
            .Register(c => new S3SqsQueue(c.Resolve<SqsOptions>(), c.Resolve<SqsQueueUrlOptions>(), c.Resolve<SqsMessagesBlobClient>()))
            .As<ISqsQueue>()
            .SingleInstance();
    }
}

//public sealed class DevelopmentSqsHandlersModule : Module
//{
//    private class DevelopmentSqsQueuePublisher : ISqsQueuePublisher
//    {
//        private readonly ILogger<DevelopmentSqsQueuePublisher> _logger;

//        public DevelopmentSqsQueuePublisher(ILogger<DevelopmentSqsQueuePublisher> logger)
//        {
//            _logger = logger;
//        }

//        public Task<bool> CopyToQueue<T>(string queueUrl, T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
//        {
//            _logger.LogInformation("Simulating to copy message to queue (for Development only)");
//            return Task.FromResult(true);
//        }
//    }

//    private class DevelopmentSqsQueue : ISqsQueue
//    {
//        private readonly ILogger<DevelopmentSqsQueue> _logger;

//        public DevelopmentSqsQueue(ILogger<DevelopmentSqsQueue> logger)
//        {
//            _logger = logger;
//        }

//        public Task<bool> Copy<T>(T message, SqsQueueOptions queueOptions, CancellationToken cancellationToken) where T : class
//        {
//            _logger.LogInformation("Simulating to copy message to queue (for Development only)");
//            return Task.FromResult(true);
//        }
//    }

//    protected override void Load(ContainerBuilder builder)
//    {
//        builder
//            .Register(c => new DevelopmentSqsQueuePublisher(c.Resolve<ILogger<DevelopmentSqsQueuePublisher>>()))
//            .As<ISqsQueuePublisher>()
//            .SingleInstance();

//        builder
//            .Register(c => new DevelopmentSqsQueue(c.Resolve<ILogger<DevelopmentSqsQueue>>()))
//            .As<ISqsQueue>()
//            .SingleInstance();
//    }
//}
