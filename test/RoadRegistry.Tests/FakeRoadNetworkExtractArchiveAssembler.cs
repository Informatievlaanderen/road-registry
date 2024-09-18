namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Extracts;

public class FakeRoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
{
    public Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new MemoryStream());
    }
}
