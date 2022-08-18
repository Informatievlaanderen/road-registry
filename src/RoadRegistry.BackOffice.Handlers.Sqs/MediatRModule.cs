namespace RoadRegistry.BackOffice.Handlers.Sqs;

using Autofac;
using MediatR;
using MediatR.Pipeline;

public class MediatRModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var mediatrOpenTypes = new[]
        {
            typeof(IRequestHandler<,>),
            typeof(IRequestExceptionHandler<,,>),
            typeof(IRequestExceptionAction<,>),
            typeof(INotificationHandler<>),
            typeof(IStreamRequestHandler<,>)
        };

        foreach (var mediatrOpenType in mediatrOpenTypes)
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                // when having a single class implementing several handler types
                // this call will cause a handler to be called twice
                // in general you should try to avoid having a class implementing for instance `IRequestHandler<,>` and `INotificationHandler<>`
                // the other option would be to remove this call
                // see also https://github.com/jbogard/MediatR/issues/462
                .AsImplementedInterfaces();

        builder.RegisterGeneric(typeof(Extracts.UploadExtractFeatureCompareRequestValidator)).As(typeof(IPipelineBehavior<,>));
        builder.RegisterGeneric(typeof(Uploads.UploadExtractFeatureCompareRequestValidator)).As(typeof(IPipelineBehavior<,>));
    }
}
