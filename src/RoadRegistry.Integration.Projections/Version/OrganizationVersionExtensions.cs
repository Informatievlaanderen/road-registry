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
    using Schema.Organizations.Version;

    public static class OrganizationVersionExtensions
    {
        public static async Task CreateNewOrganizationVersion<T>(
            this IntegrationContext context,
            string code,
            Envelope<T> message,
            Action<OrganizationVersion> applyEventInfoOn,
            CancellationToken ct) where T : IMessage
        {
            var organizationVersion = await context.LatestOrganizationVersionPosition(code, ct);

            if (organizationVersion is null)
            {
                throw DatabaseItemNotFound(code);
            }

            var newOrganizationVersion = organizationVersion.CloneAndApplyEventInfo(
                message.Position,
                applyEventInfoOn);

            await context.OrganizationVersions.AddAsync(newOrganizationVersion, ct);
        }

        public static async Task<OrganizationVersion> LatestOrganizationVersionPosition(
            this IntegrationContext context,
            string code,
            CancellationToken ct)
            => context
                   .OrganizationVersions
                   .Local
                   .Where(x => x.Code == code)
                   .MaxBy(x => x.Position)
               ?? await context
                   .OrganizationVersions
                   .AsNoTracking()
                   .Where(x => x.Code == code)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        private static ProjectionItemNotFoundException<OrganizationVersionProjection> DatabaseItemNotFound(string code)
            => new(code);
    }
}
