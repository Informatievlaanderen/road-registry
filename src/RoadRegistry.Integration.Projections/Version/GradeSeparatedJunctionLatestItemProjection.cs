namespace RoadRegistry.Integration.Projections.Version;

using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;
using Schema.GradeSeparatedJunctions.Version;

public class GradeSeparatedJunctionVersionProjection : ConnectedProjection<IntegrationContext>
{
    public GradeSeparatedJunctionVersionProjection()
    {
        When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
        {
            var typeTranslation = GradeSeparatedJunctionType.Parse(envelope.Message.Type).Translation;

            var junctionVersion = new GradeSeparatedJunctionVersion
            {
                Position = envelope.Position,
                Id = envelope.Message.Id,
                UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId,
                LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
                TypeId = typeTranslation.Identifier,
                TypeLabel = typeTranslation.Name,
                OrganizationId = envelope.Message.Origin.OrganizationId,
                OrganizationName = envelope.Message.Origin.Organization,
                CreatedOnTimestamp = envelope.Message.Origin.Since,
                VersionTimestamp = envelope.Message.Origin.Since
            };

            await context.GradeSeparatedJunctionVersions.AddAsync(junctionVersion, token);
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
        var junctionVersion = await context.LatestGradeSeparatedJunctionVersionPosition(
            added.Id,
            token);

        var typeTranslation = GradeSeparatedJunctionType.Parse(added.Type).Translation;

        if (junctionVersion is null)
        {
            junctionVersion = new GradeSeparatedJunctionVersion
            {
                Position = envelope.Position,
                Id = added.Id,
                UpperRoadSegmentId = added.UpperRoadSegmentId,
                LowerRoadSegmentId = added.LowerRoadSegmentId,
                TypeId = typeTranslation.Identifier,
                TypeLabel = typeTranslation.Name,
                OrganizationId = envelope.Message.OrganizationId,
                OrganizationName = envelope.Message.Organization,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            };
        }
        else
        {
            junctionVersion.UpperRoadSegmentId = added.UpperRoadSegmentId;
            junctionVersion.LowerRoadSegmentId = added.LowerRoadSegmentId;
            junctionVersion.TypeId = typeTranslation.Identifier;
            junctionVersion.TypeLabel = typeTranslation.Name;
            junctionVersion.OrganizationId = envelope.Message.OrganizationId;
            junctionVersion.OrganizationName = envelope.Message.Organization;
            junctionVersion.IsRemoved = false;
            junctionVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        }

        await context.GradeSeparatedJunctionVersions.AddAsync(junctionVersion, token);
    }

    private static async Task ModifyGradeSeparatedJunction(
        IntegrationContext context,
        GradeSeparatedJunctionModified modified,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await context.CreateNewGradeSeparatedJunctionVersion(
            modified.Id,
            envelope,
            newVersion =>
            {
                var typeTranslation = GradeSeparatedJunctionType.Parse(modified.Type).Translation;

                newVersion.UpperRoadSegmentId = modified.UpperRoadSegmentId;
                newVersion.LowerRoadSegmentId = modified.LowerRoadSegmentId;
                newVersion.TypeId = typeTranslation.Identifier;
                newVersion.TypeLabel = typeTranslation.Name;
                newVersion.OrganizationId = envelope.Message.OrganizationId;
                newVersion.OrganizationName = envelope.Message.Organization;
                newVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            },
            token);
    }

    private static async Task RemoveGradeSeparatedJunction(
        IntegrationContext context,
        GradeSeparatedJunctionRemoved removed,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await context.CreateNewGradeSeparatedJunctionVersion(
            removed.Id,
            envelope,
            newVersion =>
            {
                newVersion.OrganizationId = envelope.Message.OrganizationId;
                newVersion.OrganizationName = envelope.Message.Organization;
                newVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
                newVersion.IsRemoved = true;
            },
            token);
    }
}
