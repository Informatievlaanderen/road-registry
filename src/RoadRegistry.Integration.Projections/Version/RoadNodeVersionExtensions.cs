namespace RoadRegistry.Integration.Projections.Version
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using Schema;
    using Schema.RoadNodes.Version;

    public static class RoadNodeVersionExtensions
    {
        public static async Task CreateNewRoadNodeVersion<T>(
            this IntegrationContext context,
            int id,
            Envelope<T> message,
            Action<RoadNodeVersion> applyEventInfoOn,
            CancellationToken ct) where T : IMessage
        {
            var roadNodeVersion = await context.LatestRoadNodeVersionPosition(id, ct);

            if (roadNodeVersion is null)
            {
                throw DatabaseItemNotFound(id);
            }

            var newRoadNodeVersion = roadNodeVersion.CloneAndApplyEventInfo(
                message.Position,
                applyEventInfoOn);

            await context.RoadNodeVersions.AddAsync(newRoadNodeVersion, ct);
        }

        public static async Task<RoadNodeVersion> LatestRoadNodeVersionPosition(
            this IntegrationContext context,
            int id,
            CancellationToken ct)
            => context
                   .RoadNodeVersions
                   .Local
                   .Where(x => x.Id == id)
                   .MaxBy(x => x.Position)
               ?? await context
                   .RoadNodeVersions
                   .AsNoTracking()
                   .Where(x => x.Id == id)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<RoadNodeVersionProjection> DatabaseItemNotFound(int id)
            => new(id.ToString());
    }
}
