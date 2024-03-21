namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using Autofac;
using Autofac.Builder;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Configuration;
using MediatR;
using Microsoft.Extensions.Configuration;

public static class ContainerBuilderExtensions
{
    public static IRegistrationBuilder<IConfiguration, SimpleActivatorData, SingleRegistrationStyle> RegisterConfiguration(this ContainerBuilder builder, IConfiguration configuration)
    {
        return builder
            .Register(c => configuration)
            .AsSelf()
            .As<IConfiguration>()
            .SingleInstance();
    }

    public static IRegistrationBuilder<IIdempotentCommandHandler, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterIdempotentCommandHandler(this ContainerBuilder builder)
    {
        return builder
            .RegisterType<RoadRegistryIdempotentCommandHandler>()
            .As<IIdempotentCommandHandler>()
            .AsSelf()
            .InstancePerLifetimeScope();
    }

    public static ContainerBuilder RegisterMediator(this ContainerBuilder builder)
    {
        builder
            .RegisterType<Mediator>()
            .As<IMediator>()
            .InstancePerLifetimeScope();

        return builder;
    }

    public static ContainerBuilder RegisterRetryPolicy(this ContainerBuilder builder)
    {
        builder.RegisterOptions<RetryPolicyOptions>();

        builder.Register(c =>
            {
                var retryPolicyOptions = c.Resolve<RetryPolicyOptions>();
                return new LambdaHandlerRetryPolicy(retryPolicyOptions.MaxRetryCount, retryPolicyOptions.StartingRetryDelaySeconds);
            })
            .As<ICustomRetryPolicy>()
            .AsSelf()
            .SingleInstance();

        return builder;
    }
}
