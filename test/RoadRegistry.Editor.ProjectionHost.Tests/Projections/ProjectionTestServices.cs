namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using BackOffice;
using Microsoft.IO;

public class ProjectionTestServices
{
    public ProjectionTestServices()
    {
        MemoryStreamManager = new RecyclableMemoryStreamManager();
        FileEncoding = FileEncoding.UTF8;
    }

    public RecyclableMemoryStreamManager MemoryStreamManager { get; }
    public FileEncoding FileEncoding { get; }
}
