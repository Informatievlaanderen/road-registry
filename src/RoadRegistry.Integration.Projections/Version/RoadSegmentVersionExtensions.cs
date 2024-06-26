namespace RoadRegistry.Integration.Projections.Version
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using RoadRegistry.Integration.Schema.RoadSegments.Version;
    using Schema;

    public static class RoadSegmentVersionExtensions
    {
        public static async Task<RoadSegmentVersion> CreateNewRoadSegmentVersion<T>(
            this IntegrationContext context,
            int roadSegmentId,
            Envelope<T> message,
            CancellationToken ct) where T : IMessage
        {
            var roadSegmentVersion = await context.LatestPosition(roadSegmentId, ct);

            if (roadSegmentVersion is null)
            {
                throw DatabaseItemNotFound(roadSegmentId);
            }

            var newRoadSegmentVersion = roadSegmentVersion.Clone(message.Position);
            return newRoadSegmentVersion;
        }

        private static async Task<RoadSegmentVersion> LatestPosition(
            this IntegrationContext context,
            int roadSegmentId,
            CancellationToken ct)
            => context
                   .RoadSegmentVersions
                   .Local
                   .Where(x => x.Id == roadSegmentId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .RoadSegmentVersions
                   .AsNoTracking()
                   .Where(x => x.Id == roadSegmentId)
                   .Include(x => x.Lanes)
                   .Include(x => x.Surfaces)
                   .Include(x => x.Widths)
                   //.Include(x => x.BuildingUnits) //TODO-rik add lanes/surfaces/...?
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<RoadSegmentVersionProjection> DatabaseItemNotFound(int roadSegmentId)
            => new(roadSegmentId.ToString());
    }
}
