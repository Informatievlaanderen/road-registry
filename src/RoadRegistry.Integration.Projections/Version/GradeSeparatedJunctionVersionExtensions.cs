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
    using Schema.GradeSeparatedJunctions.Version;

    public static class GradeSeparatedJunctionVersionExtensions
    {
        public static async Task CreateNewGradeSeparatedJunctionVersion<T>(
            this IntegrationContext context,
            int id,
            Envelope<T> message,
            Action<GradeSeparatedJunctionVersion> applyEventInfoOn,
            CancellationToken ct) where T : IMessage
        {
            var roadNodeVersion = await context.LatestGradeSeparatedJunctionVersionPosition(id, ct);

            if (roadNodeVersion is null)
            {
                throw DatabaseItemNotFound(id);
            }

            var newGradeSeparatedJunctionVersion = roadNodeVersion.CloneAndApplyEventInfo(
                message.Position,
                applyEventInfoOn);

            await context.GradeSeparatedJunctionVersions.AddAsync(newGradeSeparatedJunctionVersion, ct);
        }

        public static async Task<GradeSeparatedJunctionVersion> LatestGradeSeparatedJunctionVersionPosition(
            this IntegrationContext context,
            int id,
            CancellationToken ct)
            => context
                   .GradeSeparatedJunctionVersions
                   .Local
                   .Where(x => x.Id == id)
                   .MaxBy(x => x.Position)
               ?? await context
                   .GradeSeparatedJunctionVersions
                   .AsNoTracking()
                   .Where(x => x.Id == id)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<GradeSeparatedJunctionVersionProjection> DatabaseItemNotFound(int id)
            => new(id.ToString());
    }
}
