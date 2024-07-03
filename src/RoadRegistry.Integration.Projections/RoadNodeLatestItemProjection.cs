namespace RoadRegistry.Integration.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NodaTime;
using Schema;
using Schema.RoadNodes;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

public class RoadNodeLatestItemProjection : ConnectedProjection<IntegrationContext>
{
    public RoadNodeLatestItemProjection()
    {
        When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
        {
            var typeTranslation = RoadNodeType.Parse(envelope.Message.Type).Translation;

            var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
            var pointShapeContent = new PointShapeContent(point);

            await context.RoadNodes.AddAsync(new RoadNodeLatestItem
            {
                Id = envelope.Message.Id,
                TypeId = typeTranslation.Identifier,
                TypeLabel = typeTranslation.Name,
                Version = envelope.Message.Version,
                OrganizationId = envelope.Message.Origin.OrganizationId,
                OrganizationName = envelope.Message.Origin.Organization,
                Geometry = BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry),
                CreatedOnTimestamp = envelope.Message.Origin.Since.ToBelgianInstant(),
                VersionTimestamp = envelope.Message.Origin.Since.ToBelgianInstant(),
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape)), token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var message in envelope.Message.Changes.Flatten())
                switch (message)
                {
                    case RoadNodeAdded node:
                        await AddRoadNode(context, envelope, node, token);
                        break;

                    case RoadNodeModified node:
                        await ModifyRoadNode(context, envelope, node, token);
                        break;

                    case RoadNodeRemoved node:
                        await RemoveRoadNode(context, envelope, node, token);
                        break;
                }
        });
    }

    private static async Task AddRoadNode(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadNodeAdded node,
        CancellationToken token)
    {
        var typeTranslation = RoadNodeType.Parse(node.Type).Translation;

        var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(node.Geometry));
        var pointShapeContent = new PointShapeContent(point);

        await context.RoadNodes.AddAsync(new RoadNodeLatestItem
        {
            Id = node.Id,
            TypeId = typeTranslation.Identifier,
            TypeLabel = typeTranslation.Name,
            Version = node.Version,
            OrganizationId = envelope.Message.OrganizationId ,
            OrganizationName = envelope.Message.Organization,
            Geometry = BackOffice.GeometryTranslator.Translate(node.Geometry),
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
        }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape)), token);
    }

    private static async Task ModifyRoadNode(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadNodeModified node,
        CancellationToken token)
    {
        var typeTranslation = RoadNodeType.Parse(node.Type).Translation;

        var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(node.Geometry));
        var pointShapeContent = new PointShapeContent(point);

        var roadNode = await context.RoadNodes.FindAsync(node.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadNode == null)
        {
            throw new InvalidOperationException($"RoadNodeLatestItem with id {node.Id} is not found");
        }

        roadNode.TypeId = typeTranslation.Identifier;
        roadNode.TypeLabel = typeTranslation.Name;
        roadNode.Version = node.Version;
        roadNode.OrganizationId = envelope.Message.OrganizationId;
        roadNode.OrganizationName = envelope.Message.Organization;
        roadNode.Geometry = BackOffice.GeometryTranslator.Translate(node.Geometry);
        roadNode.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        roadNode.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
    }

    private static async Task RemoveRoadNode(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadNodeRemoved node,
        CancellationToken token)
    {
        var roadNode = await context.RoadNodes.FindAsync(node.Id, cancellationToken: token).ConfigureAwait(false);

        if (roadNode is not null && !roadNode.IsRemoved)
        {
            roadNode.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            roadNode.IsRemoved = true;
        }
    }
}
