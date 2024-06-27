namespace RoadRegistry.Integration.Projections.Version;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Schema;
using Schema.RoadNodes;
using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;
using RoadNodeVersion = Schema.RoadNodes.Version.RoadNodeVersion;

public class RoadNodeVersionProjection : ConnectedProjection<IntegrationContext>
{
    public RoadNodeVersionProjection()
    {
        When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
        {
            var typeTranslation = RoadNodeType.Parse(envelope.Message.Type).Translation;

            var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
            var pointShapeContent = new PointShapeContent(point);

            await context.RoadNodeVersions.AddAsync(new RoadNodeVersion
            {
                Position = envelope.Position,
                Id = envelope.Message.Id,
                TypeId = typeTranslation.Identifier,
                TypeLabel = typeTranslation.Name,
                Version = envelope.Message.Version,
                OrganizationId = envelope.Message.Origin.OrganizationId,
                OrganizationName = envelope.Message.Origin.Organization,
                Geometry = BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry),
                CreatedOnTimestamp = new DateTimeOffset(envelope.Message.Origin.Since),
                VersionTimestamp = new DateTimeOffset(envelope.Message.Origin.Since),
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

        await context.RoadNodeVersions.AddAsync(new RoadNodeVersion
        {
            Position = envelope.Position,
            Id = node.Id,
            TypeId = typeTranslation.Identifier,
            TypeLabel = typeTranslation.Name,
            Version = node.Version,
            OrganizationId = envelope.Message.OrganizationId,
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
        await context.CreateNewRoadNodeVersion(
            node.Id,
            envelope,
            roadNodeVersion =>
            {
                var typeTranslation = RoadNodeType.Parse(node.Type).Translation;

                var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(node.Geometry));
                var pointShapeContent = new PointShapeContent(point);

                roadNodeVersion.TypeId = typeTranslation.Identifier;
                roadNodeVersion.TypeLabel = typeTranslation.Name;
                roadNodeVersion.Version = node.Version;
                roadNodeVersion.OrganizationId = envelope.Message.OrganizationId;
                roadNodeVersion.OrganizationName = envelope.Message.Organization;
                roadNodeVersion.Geometry = BackOffice.GeometryTranslator.Translate(node.Geometry);
                roadNodeVersion.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
                roadNodeVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            },
            token);
    }

    private static async Task RemoveRoadNode(
        IntegrationContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadNodeRemoved node,
        CancellationToken token)
    {
        await context.CreateNewRoadNodeVersion(
            node.Id,
            envelope,
            roadNodeVersion =>
            {
                roadNodeVersion.IsRemoved = true;
                roadNodeVersion.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
            },
            token);
    }
}
