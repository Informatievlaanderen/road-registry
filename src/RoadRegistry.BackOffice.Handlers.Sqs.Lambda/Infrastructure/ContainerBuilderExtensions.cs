namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Autofac;
using Autofac.Builder;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Options;

internal static class ContainerBuilderExtensions
{
    public static IRegistrationBuilder<ServiceFactory, SimpleActivatorData, SingleRegistrationStyle> RegisterMediator(this ContainerBuilder builder)
    {
        builder
            .RegisterType<Mediator>()
            .As<IMediator>()
            .InstancePerLifetimeScope();

        // request & notification handlers
        return builder.Register<ServiceFactory>(context =>
        {
            var ctx = context.Resolve<IComponentContext>();
            return type => ctx.Resolve(type);
        });
    }

    public static IRegistrationBuilder<ICustomRetryPolicy, SimpleActivatorData, SingleRegistrationStyle> RegisterRetryPolicy(this ContainerBuilder builder, IConfiguration configuration)
    {
        var retryPolicyOptions = configuration.GetSection<RetryPolicyOptions>(RetryPolicyOptions.ConfigurationKey);
        var maxRetryCount = retryPolicyOptions.MaxRetryCount;
        var startingDelaySeconds = retryPolicyOptions.StartingRetryDelaySeconds;

        return builder.Register(_ => new LambdaHandlerRetryPolicy(maxRetryCount, startingDelaySeconds))
            .As<ICustomRetryPolicy>()
            .AsSelf()
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
}
