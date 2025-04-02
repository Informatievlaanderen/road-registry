namespace RoadRegistry.BackOffice.Extracts;

using System.IO;
using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkExtractArchiveAssembler
{
    Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken);
}
