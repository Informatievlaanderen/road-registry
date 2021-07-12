namespace RoadRegistry.BackOffice.Extracts
{
    using System.IO;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;

    public interface IRoadNetworkExtractArchiveAssembler
    {
        Task<MemoryStream> AssembleWithin(MultiPolygon contour); //Task<(MemoryStream, RoadNetworkRevision)>
    }
}
