namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;

using BackOffice.Extracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts;

public class RoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
{
    private readonly IServiceProvider _serviceProvider;

    public RoadNetworkExtractArchiveAssembler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        if (request.ZipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2 || request.ZipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2_Inwinning)
        {
            logger.LogInformation("Using assembler for Domain V2");
            var assembler = _serviceProvider.GetRequiredService<RoadNetworkExtractArchiveAssemblerForDomainV2>();
            return assembler.AssembleArchive(request, cancellationToken);
        }
        else
        {
            logger.LogInformation("Using assembler for Domain V1");
            var assembler = _serviceProvider.GetRequiredService<RoadNetworkExtractArchiveAssemblerForDomainV1>();
            return assembler.AssembleArchive(request, cancellationToken);
        }
    }
}
