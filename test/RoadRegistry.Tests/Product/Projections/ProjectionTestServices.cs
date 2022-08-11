namespace RoadRegistry.Product.Projections;

using Microsoft.IO;

public class ProjectionTestServices
{
    public ProjectionTestServices()
    {
        MemoryStreamManager = new RecyclableMemoryStreamManager();
    }

    public RecyclableMemoryStreamManager MemoryStreamManager { get; }
}
