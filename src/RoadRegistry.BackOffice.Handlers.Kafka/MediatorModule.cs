namespace RoadRegistry.BackOffice.Handlers.Kafka;

using Abstractions;
using Autofac;
using Be.Vlaanderen.Basisregisters.Projector;
using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
using Extensions;
using MediatR.Pipeline;
using RoadRegistry.StreetNameConsumer.Projections;
using RoadRegistry.StreetNameConsumer.Schema;

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
