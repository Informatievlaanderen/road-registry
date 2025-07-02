namespace RoadRegistry.BackOffice.Api.Infrastructure.Modules;

using Autofac;
using Extracts.Handlers;

public class ApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<IfMatchHeaderValidator>()
            .As<IIfMatchHeaderValidator>()
            .AsSelf();

        builder.Register(_ => (IDownloadExtractByFileRequestItemTranslator)new DownloadExtractByFileRequestItemTranslator());
    }
}
