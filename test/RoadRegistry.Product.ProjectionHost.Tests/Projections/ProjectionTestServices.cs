namespace RoadRegistry.Product.ProjectionHost.Tests.Projections;

using Microsoft.IO;

public class ProjectionTestServices
{
    public ProjectionTestServices()
    {
        MemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public RecyclableMemoryStreamManager MemoryStreamManager { get; }
}
