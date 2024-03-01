namespace RoadRegistry.BackOffice.Handlers;

using Autofac;
using BackOffice.Extensions;
using Extracts;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterMediatrHandlersFromAssemblyContaining(GetType());

        builder.Register(c => (IDownloadExtractByFileRequestItemTranslator)new DownloadExtractByFileRequestItemTranslator());
    }
}
