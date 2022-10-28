namespace RoadRegistry.BackOffice.Handlers.Kafka;

using System.Reflection;
using Abstractions;
using Autofac;
using MediatR.Pipeline;
using MessagingHost.Kafka;
using MessagingHost.Kafka.Projections;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var mediatrOpenTypes = new[]
        {
            typeof(EndpointRequestHandler<,>),
            typeof(IRequestExceptionHandler<,,>),
            typeof(IRequestExceptionAction<,>)
        };

        foreach (var mediatrOpenType in mediatrOpenTypes)
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();

        builder
            .RegisterProjectionMigrator<StreetNameConsumerContextFactory>()
            .RegisterProjections<StreetNameConsumerProjection, StreetNameConsumerContext>(
                context => new StreetNameConsumerProjection(),
                ConnectedProjectionSettings.Default);
    }
}
