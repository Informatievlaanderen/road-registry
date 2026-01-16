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

    private IRoadNetworkExtractArchiveAssembler Create(string zipArchiveWriterVersion)
    {
        if (zipArchiveWriterVersion == WellKnownZipArchiveWriterVersions.DomainV2)
        {
            return _serviceProvider.GetRequiredService<RoadNetworkExtractArchiveAssemblerForDomainV2>();
        }

        return _serviceProvider.GetRequiredService<RoadNetworkExtractArchiveAssemblerForDomainV1>();
    }

    public Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

        var assembler = Create(request.ZipArchiveWriterVersion);
        logger.LogInformation("Using assembler of type '{Type}'", assembler.GetType().FullName);

        return assembler.AssembleArchive(request, cancellationToken);
    }
}
