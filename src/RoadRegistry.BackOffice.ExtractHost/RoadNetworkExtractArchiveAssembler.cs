namespace RoadRegistry.BackOffice.ExtractHost
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Extracts;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;

    public class RoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
    {
        private readonly RecyclableMemoryStreamManager _manager;

        public RoadNetworkExtractArchiveAssembler(RecyclableMemoryStreamManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }
        public Task<MemoryStream> AssembleWithin(MultiPolygon contour) // Task<(MemoryStream, RoadNetworkRevision)>
        {
            return Task.FromResult(_manager.GetStream());
        }
    }
}
