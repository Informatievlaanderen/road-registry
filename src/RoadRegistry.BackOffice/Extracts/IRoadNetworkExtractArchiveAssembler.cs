namespace RoadRegistry.BackOffice.Extracts;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.Extracts;

public interface IRoadNetworkExtractArchiveAssembler
{
    Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken);
}
