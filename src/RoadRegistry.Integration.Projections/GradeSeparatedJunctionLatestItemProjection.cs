namespace RoadRegistry.Integration.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public GradeSeparatedJunctionLatestItemProjection()
    {
        When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
        {
            var typeTranslation = GradeSeparatedJunctionType.Parse(envelope.Message.Type).Translation;

            var junctionRecord = new GradeSeparatedJunctionLatestItem
            {
                Id = envelope.Message.Id,
                UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId,
                LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
                TypeId = typeTranslation.Identifier,
                TypeLabel = typeTranslation.Name,
                BeginOrganizationId = envelope.Message.Origin.OrganizationId,
                BeginOrganizationName = envelope.Message.Origin.Organization,
                CreatedOnTimestamp = envelope.Message.Origin.Since,
                VersionTimestamp = envelope.Message.Origin.Since
            };

            await context.AddAsync(junctionRecord, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case GradeSeparatedJunctionAdded added:
                        await AddGradeSeparatedJunction(context, added, envelope, token);
                        break;
                    case GradeSeparatedJunctionModified modified:
                        await ModifyGradeSeparatedJunction(context, modified, envelope, token);
                        break;
                    case GradeSeparatedJunctionRemoved removed:
                        await RemoveGradeSeparatedJunction(context, removed, envelope, token);
                        break;
                }
        });
    }

    private static async Task AddGradeSeparatedJunction(
        IntegrationContext context,
        GradeSeparatedJunctionAdded added,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.GradeSeparatedJunctions
            .IncludeLocalSingleOrDefaultAsync(x => x.Id == added.Id, token)
            .ConfigureAwait(false);
        if (latestItem is null)
        {
            latestItem = new GradeSeparatedJunctionLatestItem
            {
                Id = added.Id
            };
            await context.GradeSeparatedJunctions.AddAsync(latestItem, token);
        }
        else
        {
            latestItem.IsRemoved = false;
        }

        var typeTranslation = GradeSeparatedJunctionType.Parse(added.Type).Translation;

        latestItem.UpperRoadSegmentId = added.UpperRoadSegmentId;
        latestItem.LowerRoadSegmentId = added.LowerRoadSegmentId;
        latestItem.TypeId = typeTranslation.Identifier;
        latestItem.TypeLabel = typeTranslation.Name;
        latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
        latestItem.BeginOrganizationName = envelope.Message.Organization;
        latestItem.CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task ModifyGradeSeparatedJunction(
        IntegrationContext context,
        GradeSeparatedJunctionModified modified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.GradeSeparatedJunctions.FindAsync(modified.Id, cancellationToken: token);
        if (latestItem is null)
        {
            throw new InvalidOperationException($"{nameof(GradeSeparatedJunctionLatestItem)} with id {modified.Id} is not found");
        }

        var typeTranslation = GradeSeparatedJunctionType.Parse(modified.Type).Translation;

        latestItem.UpperRoadSegmentId = modified.UpperRoadSegmentId;
        latestItem.LowerRoadSegmentId = modified.LowerRoadSegmentId;
        latestItem.TypeId = typeTranslation.Identifier;
        latestItem.TypeLabel = typeTranslation.Name;
        latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
        latestItem.BeginOrganizationName = envelope.Message.Organization;
        latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task RemoveGradeSeparatedJunction(
        IntegrationContext context,
        GradeSeparatedJunctionRemoved removed,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        var latestItem = await context.GradeSeparatedJunctions.FindAsync(removed.Id, cancellationToken: token);
        if (latestItem is not null && !latestItem.IsRemoved)
        {
            latestItem.BeginOrganizationId = envelope.Message.OrganizationId;
            latestItem.BeginOrganizationName = envelope.Message.Organization;
            latestItem.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            latestItem.IsRemoved = true;
        }
    }
}
