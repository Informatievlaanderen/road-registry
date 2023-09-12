namespace RoadRegistry.BackOffice.Handlers;

using Autofac;
using BackOffice.Extensions;
using Extracts;
using FeatureToggles;

public class MediatorModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterMediatrHandlersFromAssemblyContaining(GetType());

        builder.Register(c => (IDownloadExtractByFileRequestItemTranslator)(c.Resolve<UseNetTopologySuiteShapeWriterFeatureToggle>().FeatureEnabled
            ? new DownloadExtractByFileRequestItemTranslatorNetTopologySuite()
            : new DownloadExtractByFileRequestItemTranslator(
                WellKnownEncodings.WindowsAnsi)
            ));
    }
}
