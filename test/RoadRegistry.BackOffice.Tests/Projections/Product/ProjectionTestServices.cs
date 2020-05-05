namespace RoadRegistry.BackOffice.Projections.Product
{
    using Microsoft.IO;

    public class ProjectionTestServices
    {
        public ProjectionTestServices()
        {
            MemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public RecyclableMemoryStreamManager MemoryStreamManager { get; }
    }
}
