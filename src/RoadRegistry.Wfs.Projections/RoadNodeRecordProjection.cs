namespace RoadRegistry.Wfs.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Schema;

public class RoadNodeRecordProjection : ConnectedProjection<WfsContext>
{
    public RoadNodeRecordProjection()
    {
        When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
        {
            await context.RoadNodes.AddAsync(new RoadNodeRecord
            {
                Id = envelope.Message.Id,
                BeginTime = envelope.Message.Origin.Since,
                Type = GetRoadNodesTypeDutchTranslation(envelope.Message.Type),
                Geometry = GeometryTranslator.Translate(envelope.Message.Geometry)
            }, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadNodeAdded roadNodeAdded:
                        await AddRoadNode(context, envelope, roadNodeAdded, token);
                        break;

                    case RoadNodeModified roadNodeModified:
                        await ModifyRoadNode(context, envelope, roadNodeModified, token);
                        break;

                    case RoadNodeRemoved roadNodeRemoved:
                        await RemoveRoadNode(roadNodeRemoved, context, token);
                        break;
                }
        });
    }

    private static async Task AddRoadNode(WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadNodeAdded roadNodeAdded,
        CancellationToken token)
    {
        await context.RoadNodes.AddAsync(new RoadNodeRecord
        {
            Id = roadNodeAdded.Id,
            BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When),
            Type = GetRoadNodesTypeDutchTranslation(roadNodeAdded.Type),
            Geometry = GeometryTranslator.Translate(roadNodeAdded.Geometry)
        }, token);
    }

    private static string GetRoadNodesTypeDutchTranslation(string roadNodeType)
    {
        return RoadNodeType.CanParse(roadNodeType)
            ? RoadNodeType.Parse(roadNodeType).Translation.Name
            : roadNodeType;
    }

    private static async Task ModifyRoadNode(WfsContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadNodeModified roadNodeModified,
        CancellationToken token)
    {
        var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeModified.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadNodeRecord == null)
        {
            throw new InvalidOperationException($"RoadNodeRecord with id {roadNodeModified.Id} is not found");
        }

        roadNodeRecord.Id = roadNodeModified.Id;
        roadNodeRecord.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When);
        roadNodeRecord.Type = GetRoadNodesTypeDutchTranslation(roadNodeModified.Type);
        roadNodeRecord.Geometry = GeometryTranslator.Translate(roadNodeModified.Geometry);
    }

    private static async Task RemoveRoadNode(RoadNodeRemoved roadNodeRemoved, WfsContext context, CancellationToken token)
    {
        var roadNodeRecord = await context.RoadNodes.FindAsync(roadNodeRemoved.Id, cancellationToken: token).ConfigureAwait(false);
        if (roadNodeRecord is not null)
        {
            context.RoadNodes.Remove(roadNodeRecord);
        }
    }
}
