namespace RoadRegistry.BackOffice.Handlers;

using Autofac;
using BackOffice.Extensions;
using Extracts;
using Information;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterMediatrHandlersFromAssemblyContaining(GetType());

        builder.Register(c => (IDownloadExtractByFileRequestItemTranslator)new DownloadExtractByFileRequestItemTranslator());
        builder.Register(c => (IExtractContourValidator)new ExtractContourValidator());
    }
}
