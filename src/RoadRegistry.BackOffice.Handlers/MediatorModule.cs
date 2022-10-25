namespace RoadRegistry.BackOffice.Handlers;

using Autofac;
using Extracts;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

public class MediatorModule : Module
{
    private readonly IEnumerable<Type> _typeRegistrations = new[]
    {
        typeof(IRequestHandler<,>),
        typeof(IRequestExceptionHandler<,,>),
        typeof(IRequestExceptionAction<,>)
    };

    protected override void Load(ContainerBuilder builder)
    {
        builder.Register(context => new DownloadExtractByFileRequestItemTranslator(
            WellKnownEncodings.WindowsAnsi,
            context.Resolve<ILogger<DownloadExtractByFileRequestItemTranslator>>())
        );

        foreach (var mediatrOpenType in _typeRegistrations)
            builder
                .RegisterAssemblyTypes(GetType().Assembly)
                .AsClosedTypesOf(mediatrOpenType)
                .AsImplementedInterfaces();
    }
}