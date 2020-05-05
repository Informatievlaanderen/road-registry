namespace RoadRegistry.BackOffice.Projections.BackOffice
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
